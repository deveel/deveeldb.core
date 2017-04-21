using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Query.Plan {
	static class QueryPlanNodeExtensions {
		public static IList<ObjectName> DiscoverTableNames(this IQueryPlanNode queryPlan) {
			var tableNames = new List<ObjectName>();
			var visitor = new TableNameDicovery(tableNames);
			visitor.Visit(queryPlan);
			return tableNames;
		}

		public static IList<CorrelatedReferenceExpression> DiscoverCorrelatedReferences(this IQueryPlanNode queryPlan,
			int queryLevel) {
			return DiscoverCorrelatedReferences(queryPlan, queryLevel, new List<CorrelatedReferenceExpression>());
		}

		public static IList<CorrelatedReferenceExpression> DiscoverCorrelatedReferences(this IQueryPlanNode queryPlan,
			int queryLevel, IList<CorrelatedReferenceExpression> list) {
			var visitor = new CorrelatedExpressionDiscovery(queryLevel, list);
			visitor.Visit(queryPlan);
			return list;
		}

		#region TableNameDiscovery

		class TableNameDicovery : QueryPlanVisitor {
			private IList<ObjectName> tableNames;

			public TableNameDicovery(IList<ObjectName> tableNames) {
				this.tableNames = tableNames;
			}

			public override IQueryPlanNode Visit(IQueryPlanNode node) {
				if (node is FetchTableNode) {
					var fetchTableNode = (FetchTableNode) node;
					if (!tableNames.Contains(fetchTableNode.TableName))
						tableNames.Add(fetchTableNode.TableName);
				}

				return base.Visit(node);
			}
		}

		#endregion

		#region CorrelatedExpressionDiscovery

		class CorrelatedExpressionDiscovery : QueryPlanVisitor {
			private readonly int queryLevel;
			private IList<CorrelatedReferenceExpression> list;

			public CorrelatedExpressionDiscovery(int queryLevel, IList<CorrelatedReferenceExpression> list) {
				this.queryLevel = queryLevel;
				this.list = list;
			}

			public override IQueryPlanNode Visit(IQueryPlanNode node) {
				if (node is ConstantSelectNode) {
					list = ((ConstantSelectNode) node).Expression.DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is FunctionNode) {
					var function = (FunctionNode) node;
					foreach (var expression in function.Functions) {
						list = expression.DiscoverCorrelatedReferences(queryLevel, list);
					}
				} else if (node is FullSelectNode) {
					list = ((FullSelectNode) node).DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is GroupNode) {
					var group = (GroupNode) node;
					foreach (var function in group.Functions) {
						list = function.DiscoverCorrelatedReferences(queryLevel, list);
					}
				} else if (node is JoinNode) {
					list = ((JoinNode) node).RightExpression.DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is RangeSelectNode) {
					list = ((RangeSelectNode) node).Expression.DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is SimplePatternSelectNode) {
					list = ((SimplePatternSelectNode) node).Pattern.DiscoverCorrelatedReferences(queryLevel, list);
				} else if (node is SimpleSelectNode) {
					list = ((SimpleSelectNode) node).Expression.DiscoverCorrelatedReferences(queryLevel, list);
				}

				return base.Visit(node);
			}
		}

		#endregion
	}
}