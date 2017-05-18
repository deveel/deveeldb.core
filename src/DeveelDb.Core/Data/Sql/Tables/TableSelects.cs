using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Deveel.Data.Indexes;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Indexes;
using Deveel.Data.Text;

namespace Deveel.Data.Sql.Tables {
	static class TableSelects {
		public static async Task<ITable> SelectAsync(ITable table, IContext context, SqlExpression expression) {
			if (expression is SqlBinaryExpression) {
				var binary = (SqlBinaryExpression)expression;

				// Perform the pattern search expression on the table.
				// Split the expression,
				var leftRef = (binary.Left as SqlReferenceExpression)?.ReferenceName;
				if (leftRef != null)
					// LHS is a simple variable so do a simple select
					return await SimpleSelectAsync(table, context, leftRef, binary.ExpressionType, binary.Right);
			}

			// LHS must be a constant so we can just evaluate the expression
			// and see if we get true, false, null, etc.
			var v = await expression.ReduceToConstantAsync(context);

			// If it evaluates to NULL or FALSE then return an empty set
			if (v.IsNull || v.IsFalse)
				table = table.EmptySelect();

			return table;
		}

		public static async Task<ITable> QuantifiedSelectAsync(ITable table, IContext context, SqlQuantifyExpression expression) {

			IEnumerable<SqlExpression> list;
			bool isAll = expression.ExpressionType == SqlExpressionType.All;

			if (expression.Expression.Right is SqlConstantExpression) {
				var tob = ((SqlConstantExpression)expression.Expression.Right).Value;
				if (tob.Type is SqlArrayType) {
					var array = (SqlArray)tob.Value;
					list = array;
				} else {
					throw new Exception("The right side of a sub-query operator must be an array or a sub-query.");
				}
			} else if (expression.Expression.Right is SqlQueryExpression) {
				list = new SqlExpression[] { expression.Expression.Right };
			} else {
				throw new InvalidOperationException("The right side of a sub-query operator must be an array or a sub-query.");
			}

			if (expression.Expression.Left.ExpressionType != SqlExpressionType.Reference)
				throw new NotSupportedException("Quantified expressions over constant values not supported yet");

			var columnName = ((SqlReferenceExpression) expression.Expression.Left).ReferenceName;

			// Find the row with the name given in the condition.
			int column = table.TableInfo.Columns.IndexOf(columnName);

			if (column == -1)
				throw new ArgumentException($"Unable to find the column '{columnName.Name}' in the condition.");

			// Construct a temporary table with a single column that we are
			// comparing to.
			var col = table.TableInfo.Columns[column];
			var ttable = TemporaryTable.SingleColumnTable(col.ColumnName, col.ColumnType);

			foreach (var exp in list) {
				var rowNum = ttable.NewRow();

				var evalExp =await exp.ReduceToConstantAsync(context);
				ttable.SetValue(rowNum, 0, evalExp);
			}

			ttable.BuildIndex();

			var op = expression.Expression.ExpressionType;

			// Perform the any/all sub-query on the constant table.
			return await SelectNonCorrelatedAsync(table, new[] { columnName }, op, isAll, ttable);
		}

		public static async Task<ITable> SimpleSelectAsync(ITable table, IContext context, ObjectName columnName,
			SqlExpressionType op, SqlExpression exp) {
			// Find the row with the name given in the condition.
			int column = table.TableInfo.Columns.IndexOf(columnName);

			if (column == -1)
				throw new ArgumentException($"Unable to find the column {columnName.Name} in the condition.");

			if (!exp.IsConstant())
				throw new ArgumentException("The search expression is not constant.");

			var value = await exp.ReduceToConstantAsync(context);

			IEnumerable<long> rows;

			if (op.IsBinary()) {
				// Is the column we are searching on indexable?
				var colInfo = table.TableInfo.Columns[column];
				if (!colInfo.ColumnType.IsIndexable)
					throw new InvalidOperationException(
						$"Column {colInfo.ColumnName} of type {colInfo.ColumnType} cannot be searched.");

				rows = SelectRows(table, column, op, value);
			} else {
				throw new ArgumentException($"The expression type {op} is not a valid operator for a simple select");
			}

			return new VirtualTable(table, rows.ToArray(), column);
		}

		public static bool AnyMatchesValue(ITable table, int column, SqlExpressionType op, SqlObject value) {
			return SelectRows(table, column, op, value).Any();
		}

		public static bool AllMatchesValue(ITable table, int column, SqlExpressionType op, SqlObject value) {
			var rows = SelectRows(table, column, op, value).ToBigArray();
			return rows.Length == table.RowCount;
		}

		private static IEnumerable<long> SelectRows(ITable table, int column, SqlExpressionType op, SqlObject value) {
			// If the cell is of an incompatible type, return no results,
			var colType = table.TableInfo.Columns[column].ColumnType;
			if (!value.Type.IsComparable(colType)) {
				// Types not comparable, so return 0
				return new long[0];
			}

			var index = table.GetColumnIndex(column);

			var key = new IndexKey(value);
			if (op == SqlExpressionType.Equal)
				return index.SelectEqual(key);
			if (op == SqlExpressionType.NotEqual)
				return index.SelectNotEqual(key);
			if (op == SqlExpressionType.GreaterThan)
				return index.SelectGreater(key);
			if (op == SqlExpressionType.LessThan)
				return index.SelectLess(key);
			if (op == SqlExpressionType.GreaterThanOrEqual)
				return index.SelectGreaterOrEqual(key);
			if (op == SqlExpressionType.LessThanOrEqual)
				return index.SelectLessOrEqual(key);

			// If it's not a standard operator (such as IS, NOT IS, etc) we generate the
			// range set especially.
			var rangeSet = new IndexRangeSet();
			rangeSet = rangeSet.Intersect(op, key);
			return index.SelectRange(rangeSet.ToArray());
		}

		public static Task<ITable> SearchAsync(IContext context, ITable table, int column, SqlExpressionType op, SqlObject pattern, SqlObject escape) {
			if (pattern.IsNull)
				return Task.FromResult(table.EmptySelect());

			var s = ((ISqlString) pattern.Value).ToString();
			var stringSearch = context.Scope.Resolve<ISqlStringSearch>() ?? new SqlDefaultStringSearch();

			var escapeChar = escape == null ? '\\' : ((ISqlString) escape.Value)[0];

			if (op == SqlExpressionType.Like) {
				return stringSearch.SearchLikeAsync(table, column, s, escapeChar);
			} else if (op == SqlExpressionType.NotLike) {
				return stringSearch.SearchNotLikeAsync(table, column, s, escapeChar);
			}

			throw new NotSupportedException();
		}

		public static async Task<ITable> SelectNonCorrelatedAsync(ITable table, ObjectName[] leftColumns, SqlExpressionType op, bool isAll, ITable rightTable) {
			if (rightTable.TableInfo.Columns.Count != leftColumns.Length) {
				throw new ArgumentException($"The right table has {rightTable.TableInfo.Columns.Count} columns that is different from the specified column names ({leftColumns.Length})");
			}

			// Handle trivial case of no entries to select from
			if (table.RowCount == 0)
				return table;

			// Resolve the vars in the left table and check the references are
			// compatible.
			var sz = leftColumns.Length;
			var leftColMap = new int[sz];
			var rightColMap = new int[sz];
			for (int i = 0; i < sz; ++i) {
				leftColMap[i] = table.TableInfo.Columns.IndexOf(leftColumns[i]);
				rightColMap[i] = i;

				if (leftColMap[i] == -1)
					throw new Exception($"Invalid reference: {leftColumns[i]}");

				var leftType = table.TableInfo.Columns[leftColMap[i]].ColumnType;
				var rightType = rightTable.TableInfo.Columns[i].ColumnType;
				if (!leftType.IsComparable(rightType)) {
					throw new ArgumentException($"The type of the sub-query expression {leftColumns[i]}({leftType}) " +
					                            $"is not compatible with the sub-query type {rightType}.");
				}
			}

			IEnumerable<long> rows;

			if (isAll) {
				// ----- ALL operation -----
				// We work out as follows:
				//   For >, >= type ALL we find the highest value in 'table' and
				//   select from 'source' all the rows that are >, >= than the
				//   highest value.
				//   For <, <= type ALL we find the lowest value in 'table' and
				//   select from 'source' all the rows that are <, <= than the
				//   lowest value.
				//   For = type ALL we see if 'table' contains a single value.  If it
				//   does we select all from 'source' that equals the value, otherwise an
				//   empty table.
				//   For <> type ALL we use the 'not in' algorithm.
				switch (op) {
					case SqlExpressionType.GreaterThan:
					case SqlExpressionType.GreaterThanOrEqual: {
						// Select the last from the set (the highest value),
						var highestCells = await rightTable.GetLastValuesAsync(rightColMap);
						// Select from the source table all rows that are > or >= to the
						// highest cell,
						rows = table.SelectRows(leftColMap, op, highestCells);
						break;
					}
					case SqlExpressionType.LessThan:
					case SqlExpressionType.LessThanOrEqual: {
						// Select the first from the set (the lowest value),
						var lowestCells = await rightTable.GetFirstValuesAsync(rightColMap);
						// Select from the source table all rows that are < or <= to the
						// lowest cell,
						rows = table.SelectRows(leftColMap, op, lowestCells);
						break;
					}
					case SqlExpressionType.Equal: {
						// Select the single value from the set (if there is one).
						var singleCell = await rightTable.GetSingleValuesAsync(rightColMap);
						if (singleCell != null) {
							// Select all from source_table all values that = this cell
							rows = table.SelectRows(leftColMap, op, singleCell);
						} else {
							// No single value so return empty set (no value in LHS will equal
							// a value in RHS).
							return table.EmptySelect();
						}
						break;
					}
					case SqlExpressionType.NotEqual: {
						// Equiv. to NOT IN
						rows = table.SelectRowsNotIn(rightTable, leftColMap, rightColMap);
						break;
					}
					default:
						throw new ArgumentException($"Operator of type {op} is not valid in ALL functions.");
				}
			} else {
				// ----- ANY operation -----
				// We work out as follows:
				//   For >, >= type ANY we find the lowest value in 'table' and
				//   select from 'source' all the rows that are >, >= than the
				//   lowest value.
				//   For <, <= type ANY we find the highest value in 'table' and
				//   select from 'source' all the rows that are <, <= than the
				//   highest value.
				//   For = type ANY we use same method from INHelper.
				//   For <> type ANY we iterate through 'source' only including those
				//   rows that a <> query on 'table' returns size() != 0.

				switch (op) {
					case SqlExpressionType.GreaterThan:
					case SqlExpressionType.GreaterThanOrEqual: {
						// Select the first from the set (the lowest value),
						var lowestCells = await rightTable.GetFirstValuesAsync(rightColMap);
						// Select from the source table all rows that are > or >= to the
						// lowest cell,
						rows = table.SelectRows(leftColMap, op, lowestCells);
						break;
					}
					case SqlExpressionType.LessThan:
					case SqlExpressionType.LessThanOrEqual: {
						// Select the last from the set (the highest value),
						var highestCells = await rightTable.GetLastValuesAsync(rightColMap);
						// Select from the source table all rows that are < or <= to the
						// highest cell,
						rows = table.SelectRows(leftColMap, op, highestCells);
							break;
					}
					case SqlExpressionType.Equal: {
						// Equiv. to IN
						rows = table.SelectRowsIn(rightTable, leftColMap, rightColMap);
							break;
					}
					case SqlExpressionType.NotEqual: {
						// Select the value that is the same of the entire column
						var cells = await rightTable.GetSingleValuesAsync(rightColMap);
						if (cells != null) {
							// All values from 'source_table' that are <> than the given cell.
							rows = table.SelectRows(leftColMap, op, cells);
						} else {
							// No, this means there are different values in the given set so the
							// query evaluates to the entire table.
							return table;
						}
						break;
					}
					default:
						throw new ArgumentException($"Operator of type {op} is not valid in ANY functions.");

				}
			}

			return new VirtualTable(table, rows.ToArray());
		}

		public static async Task<ITable> FullSelectAsync(IContext context, ITable table, SqlExpression expression) {
			var result = table;

			// Exit early if there's nothing in the table to select from
			var rowCount = table.RowCount;
			if (rowCount > 0) {
				var selectedSet = new BigList<long>(rowCount);

				foreach (var row in table) {
					var reduced = await row.ReduceExpressionAsync(context, expression);

					if (reduced.ExpressionType != SqlExpressionType.Constant)
						throw new InvalidOperationException();

					var value = ((SqlConstantExpression) reduced).Value;

					// If resolved to true then include in the selected set.
					if (!value.IsNull && value.IsTrue) {
						selectedSet.Add(row.Id.Number);
					}
				}

				result = new VirtualTable(table, selectedSet); ;
			}

			return result;
		}
	}
}