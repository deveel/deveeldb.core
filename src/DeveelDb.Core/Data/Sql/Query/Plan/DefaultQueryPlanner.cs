using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	public sealed class DefaultQueryPlanner : IQueryPlanner {
		private static readonly ObjectName FunctionTableName = new ObjectName("FUNCTIONTABLE");

		public async Task<IQueryPlanNode> PlanAsync(IContext context, QueryInfo queryInfo) {
			if (queryInfo == null)
				throw new ArgumentNullException(nameof(queryInfo));

			var queryExpression = queryInfo.Query;
			var sortColumns = queryInfo.SortColumns;
			var limit = queryInfo.Limit;

			var queryFrom = await QueryExpressionFrom.CreateAsync(context, queryExpression);
			var orderBy = new List<SortColumn>();
			if (sortColumns != null)
				orderBy.AddRange(sortColumns);

			return await PlanQuery(context, queryExpression, queryFrom, orderBy, limit);

		}

		private async Task<IQueryPlanNode> PlanQuery(IContext context, SqlQueryExpression queryExpression,
			QueryExpressionFrom queryFrom, IList<SortColumn> sortColumns, QueryLimit limit) {

			// ----- Resolve the SELECT list
			// If there are 0 columns selected, then we assume the result should
			// show all of the columns in the result.
			bool doSubsetColumn = (queryExpression.Items.Any());

			// What we are selecting
			var columns = BuildSelectColumns(queryExpression, queryFrom);

			// Prepare the column_set,
			var preparedColumns = PrepareColumns(context, columns);

			sortColumns = ResolveOrderByRefs(preparedColumns, sortColumns);

			// -----

			// Set up plans for each table in the from clause of the command.  For
			// sub-queries, we recurse.

			var tablePlanner = await CreateTablePlannerAsync(context, queryFrom);

			// -----

			// The WHERE and HAVING clauses
			var whereClause = queryExpression.Where;
			var havingClause = queryExpression.Having;

			PrepareJoins(tablePlanner, queryExpression, queryFrom, ref whereClause);

			// Prepare the WHERE and HAVING clause, qualifies all variables and
			// prepares sub-queries.
			whereClause = PrepareSearchExpression(context, queryFrom, whereClause);
			havingClause = PrepareSearchExpression(context, queryFrom, havingClause);

			// Any extra Aggregate functions that are part of the HAVING clause that
			// we need to add.  This is a list of a name followed by the expression
			// that contains the aggregate function.
			var extraAggregateFunctions = new List<SqlExpression>();
			if (havingClause != null)
				havingClause = FilterHaving(havingClause, extraAggregateFunctions, context);

			// Any GROUP BY functions,
			ObjectName[] groupByList;
			IList<SqlExpression> groupByFunctions;
			var gsz = ResolveGroupBy(queryExpression, queryFrom, context, out groupByList, out groupByFunctions);

			// Resolve GROUP MAX variable to a reference in this from set
			var groupmaxColumn = ResolveGroupMax(queryExpression, queryFrom);

			// -----

			// Now all the variables should be resolved and correlated variables set
			// up as appropriate.

			// If nothing in the FROM clause then simply evaluate the result of the
			// select
			if (queryFrom.SourceCount == 0)
				return EvaluateToSingle(preparedColumns);

			// Plan the where clause.  The returned node is the plan to evaluate the
			// WHERE clause.
			var node = tablePlanner.PlanSearchExpression(whereClause);

			SqlExpression[] defFunList;
			string[] defFunNames;
			var fsz = MakeupFunctions(preparedColumns, extraAggregateFunctions, out defFunList, out defFunNames);

			var groupInfo = new GroupInfo {
				Columns = preparedColumns,
				FunctionCount = fsz,
				FunctionNames = defFunNames,
				FunctionExpressions = defFunList,
				GroupByCount = gsz,
				GroupByNames = groupByList,
				GroupByExpressions = groupByFunctions.ToArray(),
				GroupMax = groupmaxColumn
			};

			node = PlanGroup(node, groupInfo);

			// The result column list
			var selectColumns = preparedColumns.SelectedColumns.ToList();
			int sz = selectColumns.Count;

			// Evaluate the having clause if necessary
			if (havingClause != null) {
				// Before we evaluate the having expression we must substitute all the
				// aliased variables.
				var havingExpr = havingClause;

				// TODO: this requires a visitor to modify the having expression
				havingExpr = ReplaceAliasedRef(havingExpr, selectColumns);

				var source = tablePlanner.GetSinglePlan();
				source.UpdatePlan(node);
				node = tablePlanner.PlanSearchExpression(havingExpr);
			}

			// Do we have a composite select expression to process?
			IQueryPlanNode rightComposite = null;
			if (queryExpression.NextComposite != null) {
				var compositeExpr = queryExpression.NextComposite;
				var compositeFrom = await QueryExpressionFrom.CreateAsync(context, compositeExpr.Expression);

				// Form the right plan
				rightComposite = await PlanQuery(context, compositeExpr.Expression, compositeFrom, null, null);
			}

			// Do we do a final subset column?
			ObjectName[] aliases = null;
			if (doSubsetColumn) {
				// Make up the lists
				var subsetVars = new ObjectName[sz];
				aliases = new ObjectName[sz];
				for (int i = 0; i < sz; ++i) {
					SelectColumn scol = selectColumns[i];
					subsetVars[i] = scol.InternalName;
					aliases[i] = scol.ResolvedName;
				}

				// If we are distinct then add the DistinctNode here
				if (queryExpression.Distinct)
					node = new DistinctNode(node, subsetVars);

				// Process the ORDER BY?
				// Note that the ORDER BY has to occur before the subset call, but
				// after the distinct because distinct can affect the ordering of the
				// result.
				if (rightComposite == null && sortColumns != null)
					node = PlanForOrderBy(node, sortColumns, queryFrom, selectColumns);

				// Rename the columns as specified in the SELECT
				node = new SubsetNode(node, subsetVars, aliases);
			} else {
				// Process the ORDER BY?
				if (rightComposite == null && sortColumns != null)
					node = PlanForOrderBy(node, sortColumns, queryFrom, selectColumns);
			}

			// Do we have a composite to merge in?
			if (rightComposite != null) {
				// For the composite
				node = new CompositeNode(node, rightComposite, queryExpression.NextComposite.Function, queryExpression.NextComposite.All);

				// Final order by?
				if (sortColumns != null)
					node = PlanForOrderBy(node, sortColumns, queryFrom, selectColumns);

				// Ensure a final subset node
				if (!(node is SubsetNode) && aliases != null) {
					node = new SubsetNode(node, aliases, aliases);
				}
			}

			if (limit != null)
				node = new LimitResultNode(node, limit.Offset, limit.Total);

			return node;
		}

		private PreparedSelectColumns PrepareColumns(IContext context, QuerySelectColumns columns) {
			int aggregateCount = 0;
			var functionColumns = new List<SelectColumn>();
			var preparedColumns = new List<SelectColumn>();
			foreach (var column in columns.SelectedColumns) {
				var prepared = columns.PrepareColumn(column, context, functionColumns, ref aggregateCount);
				preparedColumns.Add(prepared);
			}

			return new PreparedSelectColumns{
				SelectedColumns = preparedColumns,
				FunctionColumns = functionColumns,
				AggregateCount = aggregateCount
			};
		}

		private IQueryPlanNode EvaluateToSingle(PreparedSelectColumns columns) {
			if (columns.AggregateCount > 0)
				throw new InvalidOperationException("Invalid use of aggregate function in select with no FROM clause");

			// Make up the lists
			var selectedColumns = columns.SelectedColumns.ToList();
			int colCount = selectedColumns.Count;
			var colNames = new string[colCount];
			var expList = new SqlExpression[colCount];
			var subsetVars = new ObjectName[colCount];
			var aliases1 = new ObjectName[colCount];
			for (int i = 0; i < colCount; ++i) {
				SelectColumn scol = selectedColumns[i];
				expList[i] = scol.Expression;
				colNames[i] = scol.InternalName.Name;
				subsetVars[i] = scol.InternalName;
				aliases1[i] = scol.ResolvedName;
			}

			return new SubsetNode(new FunctionNode(new SingleRowTableNode(), expList, colNames), subsetVars, aliases1);
		}


		private QuerySelectColumns BuildSelectColumns(SqlQueryExpression expression, QueryExpressionFrom queryFrom) {
			var selectColumns = new QuerySelectColumns(queryFrom);

			foreach (var column in expression.Items) {
				// Is this a glob?  (eg. Part.* )
				if (column.IsGlob) {
					// Find the columns globbed and add to the 'selectedColumns' result.
					if (column.IsAll) {
						selectColumns.SelectAllColumnsFromAllSources();
					} else {
						// Otherwise the glob must be of the form '[table name].*'
						selectColumns.SelectAllColumnsFromSource(column.TableNamePart);
					}
				} else {
					// Otherwise must be a standard column reference.
					selectColumns.SelectSingleColumn(column);
				}
			}

			return selectColumns;
		}

		private async Task<TableSetPlan> CreateTablePlannerAsync(IContext context, QueryExpressionFrom queryFrom) {
			// Set up plans for each table in the from clause of the command.  For
			// sub-queries, we recurse.

			var tablePlanner = new TableSetPlan();

			for (int i = 0; i < queryFrom.SourceCount; i++) {
				var tableSource = queryFrom.GetTableSource(i);
				IQueryPlanNode plan;

				if (tableSource is FromTableSubQuerySource) {
					var subQuerySource = (FromTableSubQuerySource)tableSource;

					var subQueryExpr = subQuerySource.QueryExpression;
					var subQueryFrom = subQuerySource.QueryFrom;

					plan = await PlanQuery(context, subQueryExpr, subQueryFrom, null, null);

					if (!(plan is SubsetNode))
						throw new InvalidOperationException("The root node of a sub-query plan must be a subset.");

					var subsetNode = (SubsetNode)plan;
					subsetNode.SetAliasParentName(subQuerySource.AliasName);
				} else if (tableSource is FromTableDirect) {
					var directSource = (FromTableDirect)tableSource;
					plan = await directSource.GetQueryPlanAsync();
				} else {
					throw new InvalidOperationException(String.Format("The type of FROM source '{0}' is not supported.", tableSource.GetType()));
				}

				tablePlanner.AddFromTable(plan, tableSource);
			}

			return tablePlanner;
		}

		private static int MakeupFunctions(PreparedSelectColumns columnSet, IList<SqlExpression> aggregateFunctions, out SqlExpression[] functions, out string[] functionNames) {
			// Make up the functions list,
			var functionsList = columnSet.FunctionColumns;

			var completeFunList = new List<Tuple<SqlExpression, string>>();

			foreach (var column in functionsList) {
				completeFunList.Add(new Tuple<SqlExpression, string>(column.Expression, column.InternalName.Name));
			}
			
			int h = 0;
			foreach (var function in aggregateFunctions) {
				completeFunList.Add(new Tuple<SqlExpression, string>(function, $"HAVINGAGG_{h++}"));
			}

			int count = completeFunList.Count;
			functions = new SqlExpression[count];
			functionNames = new string[count];

			for (int i = 0; i < count; ++i) {
				functions[i] = completeFunList[i].Item1;
				functionNames[i] = completeFunList[i].Item2;
			}

			return functionsList.Count;
		}

		private SqlExpression PrepareSearchExpression(IContext context, QueryExpressionFrom queryFrom, SqlExpression expression) {
			// first check the expression is not null
			if (expression == null)
				return null;

			// This is used to prepare sub-queries and qualify variables in a
			// search expression such as WHERE or HAVING.

			// Prepare the sub-queries first
			expression = expression.Prepare(new QueryExpressionPreparer(this, queryFrom, context));

			// Then qualify all the variables.  Note that this will not qualify
			// variables in the sub-queries.
			expression = expression.Prepare(queryFrom.ExpressionPreparer);

			return expression;
		}

		private IQueryPlanNode PlanGroup(IQueryPlanNode node, GroupInfo groupInfo) {
			// If there is more than 1 aggregate function or there is a group by
			// clause, then we must add a grouping plan.
			if (groupInfo.Columns.AggregateCount > 0 ||
			    groupInfo.GroupByCount > 0) {
				// If there is no GROUP BY clause then assume the entire result is the
				// group.
				if (groupInfo.GroupByCount == 0) {
					node = new GroupNode(node, groupInfo.GroupMax, groupInfo.FunctionExpressions, groupInfo.FunctionNames);
				} else {
					// Do we have any group by functions that need to be planned first?
					int gfsz = groupInfo.GroupByExpressions.Length;
					if (gfsz > 0) {
						var groupFunList = new SqlExpression[gfsz];
						var groupFunName = new string[gfsz];
						for (int i = 0; i < gfsz; ++i) {
							groupFunList[i] = groupInfo.GroupByExpressions[i];
							groupFunName[i] = $"#GROUPBY-{i}";
						}

						node = new FunctionNode(node, groupFunList, groupFunName);
					}

					// Otherwise we provide the 'group_by_list' argument
					node = new GroupNode(node, groupInfo.GroupByNames, groupInfo.GroupMax, groupInfo.FunctionExpressions, groupInfo.FunctionNames);
				}
			} else {
				// Otherwise no grouping is occurring.  We simply need create a function
				// node with any functions defined in the SELECT.
				// Plan a FunctionsNode with the functions defined in the SELECT.
				if (groupInfo.FunctionCount > 0)
					node = new FunctionNode(node, groupInfo.FunctionExpressions, groupInfo.FunctionNames);
			}

			return node;
		}

		private static IList<SortColumn> ResolveOrderByRefs(PreparedSelectColumns columnSet, IEnumerable<SortColumn> orderBy) {
			// Resolve any numerical references in the ORDER BY list (eg.
			// '1' will be a reference to column 1.
			if (orderBy == null)
				return null;

			var columnCount = columnSet.SelectedColumns.Count();

			var resolvedColumns = new List<SortColumn>();
			foreach (var column in orderBy) {
				var resolved = column;

				var expression = column.Expression;
				if (expression.ExpressionType == SqlExpressionType.Constant) {
					var value = ((SqlConstantExpression)expression).Value;
					if (value.Type is SqlNumericType && !value.IsNull) {
						var colRef = (int) ((SqlNumber)value.Value) - 1;
						if (colRef >= 0 && colRef < columnCount) {
							var funArray = columnSet.FunctionColumns.ToArray();
							var refExp = funArray[colRef];

							resolved = new SortColumn(refExp.Expression, column.Ascending);
						}
					}
				}

				resolvedColumns.Add(resolved);
			}

			return resolvedColumns.ToArray();
		}

		private static IQueryPlanNode PlanForOrderBy(IQueryPlanNode plan, IList<SortColumn> orderBy, QueryExpressionFrom queryFrom, IList<SelectColumn> selectedColumns) {
			// Sort on the ORDER BY clause
			if (orderBy.Count > 0) {
				int sz = orderBy.Count;
				var orderList = new ObjectName[sz];
				var ascendingList = new bool[sz];

				var functionOrders = new List<SqlExpression>();

				for (int i = 0; i < sz; ++i) {
					var column = orderBy[i];
					SqlExpression exp = column.Expression;
					ascendingList[i] = column.Ascending;
					var colRef = exp.AsReference();

					if (colRef != null) {
						var newRef = queryFrom.ResolveReference(colRef);
						if (newRef == null)
							throw new InvalidOperationException($"Could not resolve ORDER BY column '{colRef}' in expression");

						newRef = ReplaceAliasedRef(newRef, selectedColumns);
						orderList[i] = newRef;
					} else {
						// Otherwise we must be ordering by an expression such as
						// '0 - a'.

						// Resolve the expression,
						exp = exp.Prepare(queryFrom.ExpressionPreparer);

						// Make sure we substitute any aliased columns in the order by
						// columns.
						exp = ReplaceAliasedRef(exp, selectedColumns);

						// The new ordering functions are called 'FUNCTIONTABLE.#ORDER-n'
						// where n is the number of the ordering expression.
						orderList[i] = new ObjectName(FunctionTableName, $"#ORDER-{functionOrders.Count}");
						functionOrders.Add(exp);
					}
				}

				// If there are functional orderings,
				// For this we must define a new FunctionTable with the expressions,
				// then order by those columns, and then use another SubsetNode
				// command node.
				int fsz = functionOrders.Count;
				if (fsz > 0) {
					var functions = new SqlExpression[fsz];
					var functionNames = new String[fsz];
					for (int n = 0; n < fsz; ++n) {
						functions[n] = functionOrders[n];
						functionNames[n] = $"#ORDER-{n}";
					}

					if (plan is SubsetNode) {
						// If the top plan is a SubsetNode then we use the
						//   information from it to create a new SubsetNode that
						//   doesn't include the functional orders we have attached here.
						var topSubsetNode = (SubsetNode)plan;
						var mappedNames = topSubsetNode.Aliases;

						// Defines the sort functions
						plan = new FunctionNode(plan, functions, functionNames);
						// Then plan the sort
						plan = new SortNode(plan, orderList, ascendingList);
						// Then plan the subset
						plan = new SubsetNode(plan, mappedNames, mappedNames);
					} else {
						// Defines the sort functions
						plan = new FunctionNode(plan, functions, functionNames);
						// Plan the sort
						plan = new SortNode(plan, orderList, ascendingList);
					}

				} else {
					// No functional orders so we only need to sort by the columns
					// defined.
					plan = new SortNode(plan, orderList, ascendingList);
				}
			}

			return plan;
		}

		private void PrepareJoins(TableSetPlan tablePlanner, SqlQueryExpression queryExpression, QueryExpressionFrom queryFrom, ref SqlExpression searchExpression) {
			var fromClause = queryExpression.From;

			bool allInner = true;
			for (int i = 0; i < fromClause.JoinPartCount; i++) {
				var joinPart = fromClause.GetJoinPart(i);
				if (joinPart.JoinType != JoinType.Inner)
					allInner = false;
			}

			for (int i = 0; i < fromClause.JoinPartCount; i++) {
				var joinPart = fromClause.GetJoinPart(i);

				var joinType = joinPart.JoinType;
				var onExpression = joinPart.OnExpression;

				if (allInner) {
					// If the whole join set is inner joins then simply move the on
					// expression (if there is one) to the WHERE clause.
					if (searchExpression != null && onExpression != null) {
						searchExpression = SqlExpression.And(searchExpression, onExpression);
					} else if (searchExpression == null) {
						searchExpression = onExpression;
					}
				} else {
					// Not all inner joins,
					if (joinType == JoinType.Inner && onExpression == null) {
						// Regular join with no ON expression, so no preparation necessary
					} else {
						// Either an inner join with an ON expression, or an outer join with
						// ON expression
						if (onExpression == null)
							throw new InvalidOperationException(String.Format("Join of type {0} requires ON expression.", joinType));

						// Resolve the on_expression
						onExpression = onExpression.Prepare(queryFrom.ExpressionPreparer);
						// And set it in the planner
						tablePlanner.JoinAt(i, joinType, onExpression);
					}
				}
			}
		}

		private static SqlExpression FilterHaving(SqlExpression havingExpression, IList<SqlExpression> aggregates, IContext context) {
			if (havingExpression is SqlBinaryExpression) {
				var binary = (SqlBinaryExpression)havingExpression;
				var expType = binary.ExpressionType;
				var newLeft = FilterHaving(binary.Left, aggregates, context);
				var newRight = FilterHaving(binary.Right, aggregates, context);
				return SqlExpression.Binary(expType, newLeft, newRight);
			}

			// Not logical so determine if the expression is an aggregate or not
			if (havingExpression.HasAggregate(context)) {
				// Has aggregate functions so we must WriteByte this expression on the
				// aggregate list.

				aggregates.Add(havingExpression);

				var name = new ObjectName(FunctionTableName, $"HAVINGAGG_{aggregates.Count}");
				return SqlExpression.Reference(name);
			}

			return havingExpression;
		}

		private static ObjectName ResolveGroupMax(SqlQueryExpression queryExpression, QueryExpressionFrom queryFrom) {
			var groupMax = queryExpression.GroupMax;
			if (groupMax != null) {
				var variable = queryFrom.ResolveReference(groupMax);
				if (variable == null)
					throw new InvalidOperationException($"The GROUP MAX column '{groupMax}' was not found.");

				groupMax = variable;
			}

			return groupMax;
		}

		private int ResolveGroupBy(SqlQueryExpression queryExpression, QueryExpressionFrom queryFrom, IContext context, out ObjectName[] columnNames, out IList<SqlExpression> expressions) {
			var groupBy = queryExpression.GroupBy == null
				? new List<SqlExpression>(0)
				: queryExpression.GroupBy.ToList();

			var groupBySize = groupBy.Count;

			expressions = new List<SqlExpression>();
			columnNames = new ObjectName[groupBySize];

			for (int i = 0; i < groupBySize; i++) {
				var expression = groupBy[i];

				// Prepare the group by expression
				expression = expression.Prepare(queryFrom.ExpressionPreparer);

				var columnName = expression.AsReference();
				if (columnName != null)
					expression = queryFrom.FindExpression(columnName);

				if (expression != null) {
					if (expression.HasAggregate(context))
						throw new InvalidOperationException($"Aggregate expression '{expression}' is not allowed in a GROUP BY clause");

					expressions.Add(expression);
					columnName = new ObjectName(FunctionTableName, $"#GROUPBY-{expressions.Count - 1}");
				}

				columnNames[i] = columnName;
			}

			return groupBySize;
		}

		private static SqlExpression ReplaceAliasedRef(SqlExpression exp, IList<SelectColumn> selectColumns) {
			var visitor = new ReferenceAliasReplacer(selectColumns.ToDictionary(x => x.ResolvedName, y => y.InternalName));
			return visitor.Visit(exp);
		}

		private static ObjectName ReplaceAliasedRef(ObjectName columnRef, IList<SelectColumn> selectColumns) {
			return ReplaceAliasedRef(columnRef, selectColumns.ToDictionary(x => x.ResolvedName, y => y.InternalName));
		}

		private static ObjectName ReplaceAliasedRef(ObjectName columnRef, IDictionary<ObjectName, ObjectName> references) {
			ObjectName internalName;
			if (references.TryGetValue(columnRef, out internalName))
				return internalName;

			return columnRef;
		}


		#region RefererenceAliasReplacer

		class ReferenceAliasReplacer : SqlExpressionVisitor {
			private IDictionary<ObjectName, ObjectName> references;

			public ReferenceAliasReplacer(IDictionary<ObjectName, ObjectName> references) {
				this.references = references;
			}

			public override SqlExpression VisitReference(SqlReferenceExpression expression) {
				var name = ReplaceAliasedRef(expression.ReferenceName, references);
				return SqlExpression.Reference(name);
			}
		}

		#endregion

		#region QueryExpressionPreparer

		class QueryExpressionPreparer : ISqlExpressionPreparer {
			private readonly DefaultQueryPlanner planner;
			private readonly QueryExpressionFrom parent;
			private readonly IContext context;

			public QueryExpressionPreparer(DefaultQueryPlanner planner, QueryExpressionFrom parent, IContext context) {
				this.planner = planner;
				this.parent = parent;
				this.context = context;
			}

			public bool CanPrepare(SqlExpression expression) {
				return expression is SqlQueryExpression;
			}

			public SqlExpression Prepare(SqlExpression expression) {
				var queryExpression = (SqlQueryExpression)expression;
				// TODO: find a way to create this async: maybe make Prepare method async?
				var queryFrom = QueryExpressionFrom.CreateAsync(context, queryExpression).Result;
				queryFrom.Parent = parent;
				var plan = planner.PlanQuery(context, queryExpression, queryFrom, null, null).Result;
				return SqlExpression.Constant(new SqlObject(new SqlQueryType(), new CacheNode(plan)));
			}
		}

		#endregion

		#region GroupInfo

		class GroupInfo {
			public PreparedSelectColumns Columns { get; set; }

			public ObjectName GroupMax { get; set; }

			public int GroupByCount { get; set; }

			public ObjectName[] GroupByNames { get; set; }

			public SqlExpression[] GroupByExpressions { get; set; }

			public int FunctionCount { get; set; }

			public string[] FunctionNames { get; set; }

			public SqlExpression[] FunctionExpressions { get; set; }
		}

		#endregion

		#region PreparedSelectColumns

		class PreparedSelectColumns {
			public IList<SelectColumn> SelectedColumns { get; set; }

			public IList<SelectColumn> FunctionColumns { get; set; }

			public int AggregateCount { get; set; }
		}

		#endregion
	}
}