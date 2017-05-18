// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Indexes;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Indexes;
using Deveel.Data.Sql.Query;

namespace Deveel.Data.Sql.Tables {
	public static class TableExtensions {
		public static SqlObject GetValue(this ITable table, long row, int column) {
			return table.GetValueAsync(row, column).Result;
		}

		internal static RawTableInfo GetRawTableInfo(this ITable table, RawTableInfo rootInfo) {
			if (table is IVirtualTable)
				return ((IVirtualTable)table).GetRawTableInfo(rootInfo);
			if (table is IRootTable)
				return ((IRootTable) table).GetRawTableInfo(rootInfo);

			throw new NotSupportedException();
		}

		internal static RawTableInfo GetRawTableInfo(this ITable table)
			=> table.GetRawTableInfo(new RawTableInfo());

		internal static RawTableInfo GetRawTableInfo(this IRootTable table, RawTableInfo rootInfo) {
			var rows = table.Select(x => x.Id.Number).ToBigList();
			rootInfo.Add(table, rows);

			return rootInfo;
		}

		internal static IEnumerable<long> ResolveRows(this ITable table, int column, IEnumerable<long> rows, ITable ancestor) {
			if (table is IVirtualTable)
				return ((IVirtualTable) table).ResolveRows(column, rows, ancestor);

			if (table != ancestor)
				throw new InvalidOperationException();

			return rows;
		}

		internal static Index GetColumnIndex(this ITable table, int column, int originalColumn, ITable ancestor) {
			if (table is IVirtualTable)
				return ((IVirtualTable) table).GetColumnIndex(column, column, ancestor);

			throw new NotSupportedException();
		}

		public static Row GetRow(this ITable table, long row) {
			return new Row(table, row);
		}

		public static Row NewRow(this ITable table) {
			return new Row(table, -1);
		}

		public static ITable Limit(this ITable table, long offset, long count) {
			return new LimitedTable(table, offset, count);
		}

		public static ITable Limit(this ITable table, long count) {
			return table.Limit(0, count);
		}

		public static ITable Subset(this ITable table, ObjectName[] columnNames, ObjectName[] aliases) {
			var columnMap = new int[columnNames.Length];

			for (int i = 0; i < columnMap.Length; i++) {
				var columnName = columnNames[i];

				var offset = table.TableInfo.Columns.IndexOf(columnName);

				if (offset < 0)
					throw new InvalidOperationException($"The column '{columnName}' was not found in table '{table.TableInfo.TableName}'.");

				columnMap[i] = offset;
			}

			return new SubsetTable(table, columnMap, aliases);
		}

		public static ITable Union(this ITable table, ITable other) {
			return TableComposites.Union(table, other);
		}

		#region GetValue

		public static Task<SqlObject> GetValueAsync(this ITable table, long row, string columnName) {
			var column = table.TableInfo.Columns.IndexOf(columnName);
			if (column < 0)
				throw new ArgumentException($"Could not find column '{columnName}' in context");

			return table.GetValueAsync(row, column);
		}


		public static async Task<SqlObject[]> GetLastValuesAsync(this ITable table, int[] columns) {
			if (columns.Length > 1)
				throw new ArgumentException("Multi-column gets not supported.");

			return new[] { await table.GetLastValueAsync(columns[0]) };
		}

		public static async Task<SqlObject> GetLastValueAsync(this ITable table, int column) {
			var rows = table.SelectLastRows(column).ToBigArray();
			if (rows.Length > 0)
				return await table.GetValueAsync(rows[0], column);

			return SqlObject.NullOf(table.TableInfo.Columns[column].ColumnType);
		}

		public static async Task<SqlObject[]> GetFirstValuesAsync(this ITable table, int[] columns) {
			if (columns.Length > 1)
				throw new ArgumentException("Multi-column gets not supported.");

			return new[] { await table.GetFirstValueAsync(columns[0]) };
		}

		public static async Task<SqlObject> GetFirstValueAsync(this ITable table, int column) {
			var rows = table.SelectFirstRows(column).ToBigArray();
			if (rows.Length > 0)
				return await table.GetValueAsync(rows[0], column);

			return SqlObject.NullOf(table.TableInfo.Columns[column].ColumnType);
		}

		public static async Task<SqlObject> GetSingleValueAsync(this ITable table, int column) {
			var rows = table.SelectFirstRows(column).ToBigArray();
			var sz = rows.Length;
			if (sz == table.RowCount && sz > 0)
				return await table.GetValueAsync(rows[0], column);

			return SqlObject.NullOf(table.TableInfo.Columns[column].ColumnType);
		}

		public static async Task<SqlObject[]> GetSingleValuesAsync(this ITable table, int[] columns) {
			if (columns.Length > 1)
				throw new ArgumentException("Multi-column gets not supported.");

			return new[] { await table.GetSingleValueAsync(columns[0]) };
		}

		#endregion


		#region Select

		#region Rows

		public static bool AnyMatches(this ITable table, int column, SqlExpressionType opType, SqlObject value) {
			return TableSelects.AnyMatchesValue(table, column, opType, value);
		}

		public static bool AllMatch(this ITable table, int column, SqlExpressionType opType, SqlObject value) {
			return TableSelects.AllMatchesValue(table, column, opType, value);
		}

		public static IEnumerable<long> SelectAllRows(this ITable table, int column) {
			return table.GetColumnIndex(column).SelectAll();
		}

		public static IEnumerable<long> SelectAllRows(this ITable table) {
			return table.Select(x => x.Id.Number);
		}

		public static async Task<IEnumerable<long>> SelectRowsAsync(this ITable table, IContext context, SqlBinaryExpression expression) {
			var objRef = expression.Left as SqlReferenceExpression;
			if (objRef == null)
				throw new NotSupportedException();

			var columnName = objRef.ReferenceName;

			var column = table.TableInfo.Columns.IndexOf(columnName);
			if (column < 0)
				throw new InvalidOperationException();

			var reduced = await expression.Right.ReduceAsync(context);
			if (reduced.ExpressionType != SqlExpressionType.Constant)
				throw new InvalidOperationException();

			var value = ((SqlConstantExpression)reduced).Value;
			var binOperator = expression.ExpressionType;

			return table.SelectRows(column, binOperator, value);
		}


		public static IEnumerable<long> SelectRows(this ITable table, int[] columnOffsets, SqlExpressionType op, SqlObject[] values) {
			if (columnOffsets.Length > 1)
				throw new NotSupportedException("Multi-column selects not supported yet.");

			return SelectRows(table, columnOffsets[0], op, values[0]);
		}

		public static IEnumerable<long> SelectRows(this ITable table, int column, SqlExpressionType op, SqlObject value) {
			// If the cell is of an incompatible type, return no results,
			var colType = table.TableInfo.Columns[column].ColumnType;
			if (!value.Type.IsComparable(colType)) {
				// Types not comparable, so return 0
				return new long[0];
			}

			// Get the selectable scheme for this column
			var index = table.GetColumnIndex(column);

			// If the operator is a standard operator, use the interned SelectableScheme
			// methods.
			var key = new IndexKey(value);
			if (op == SqlExpressionType.Equal)
				return index.SelectEqual(key);
			if (op == SqlExpressionType.NotEqual)
				return index.SelectNotEqual(key);
			if (op == SqlExpressionType.GreaterThan)
				return index.SelectGreater(key);
			if (op == SqlExpressionType.LessThan)
				return index.SelectLess(key);
			if (op == SqlExpressionType.LessThanOrEqual)
				return index.SelectGreaterOrEqual(key);
			if (op == SqlExpressionType.LessThanOrEqual)
				return index.SelectLessOrEqual(key);

			// If it's not a standard operator (such as IS, NOT IS, etc) we generate the
			// range set especially.
			var rangeSet = new IndexRangeSet();
			rangeSet = rangeSet.Intersect(op, key);
			return index.SelectRange(rangeSet.ToArray());
		}

		public static IEnumerable<long> SelectLastRows(this ITable table, int column) {
			return table.GetColumnIndex(column).SelectLast();
		}

		public static IEnumerable<long> SelectFirstRows(this ITable table, int column) {
			return table.GetColumnIndex(column).SelectFirst();
		}

		public static IEnumerable<long> SelectRowsIn(this ITable table, ITable other, int[] t1Cols, int[] t2Cols) {
			if (t1Cols.Length > 1)
				throw new NotSupportedException("Multi-column 'in' not supported yet.");

			return table.SelectRowsIn(other, t1Cols[0], t2Cols[0]);
		}

		public static IEnumerable<long> SelectRowsIn(this ITable table, ITable other, int column1, int column2) {
			// First pick the the smallest and largest table.  We only want to iterate
			// through the smallest table.
			// NOTE: This optimisation can't be performed for the 'not_in' command.

			ITable smallTable;
			ITable largeTable;
			int smallColumn;
			int largeColumn;

			if (table.RowCount < other.RowCount) {
				smallTable = table;
				largeTable = other;

				smallColumn = column1;
				largeColumn = column2;

			} else {
				smallTable = other;
				largeTable = table;

				smallColumn = column2;
				largeColumn = column1;
			}

			// Iterate through the small table's column.  If we can find identical
			// cells in the large table's column, then we should include the row in our
			// final result.

			var resultRows = new BlockIndex<SqlObject, long>();
			var op = SqlExpressionType.Equal;

			foreach (var row in smallTable) {
				var cell = row.GetValue(smallColumn);

				var selectedSet = largeTable.SelectRows(largeColumn, op, cell).ToList();

				// We've found cells that are IN both columns,

				if (selectedSet.Count > 0) {
					// If the large table is what our result table will be based on, append
					// the rows selected to our result set.  Otherwise add the index of
					// our small table.  This only works because we are performing an
					// EQUALS operation.

					if (largeTable == table) {
						// Only allow unique rows into the table set.
						int sz = selectedSet.Count;
						bool rs = true;
						for (int i = 0; rs && i < sz; ++i) {
							rs = resultRows.InsertSort(selectedSet[i]);
						}
					} else {
						// Don't bother adding in sorted order because it's not important.
						resultRows.Add(row.Id.Number);
					}
				}
			}

			return resultRows;
		}

		public static IEnumerable<long> SelectRowsNotIn(this ITable table, ITable other, int[] t1Cols, int[] t2Cols) {
			if (t1Cols.Length > 1)
				throw new NotSupportedException("Multi-column 'not in' not supported yet.");

			return table.SelectRowsNotIn(other, t1Cols[0], t2Cols[0]);
		}

		public static IEnumerable<long> SelectRowsNotIn(this ITable table, ITable other, int col1, int col2) {
			// Handle trivial cases
			var t2RowCount = other.RowCount;
			if (t2RowCount == 0)
				// No rows so include all rows.
				return table.SelectAllRows(col1);

			if (t2RowCount == 1) {
				// 1 row so select all from table1 that doesn't equal the value.
				var row = other.FirstOrDefault();
				if (row == null)
					throw new InvalidOperationException("The other table is empty");

				var cell = other.GetValue(row.Id.Number, col2);
				return table.SelectRows(col1, SqlExpressionType.NotEqual, cell);
			}

			// Iterate through table1's column.  If we can find identical cell in the
			// tables's column, then we should not include the row in our final
			// result.
			var resultRows = new BigList<long>();

			foreach (var row in table) {
				var cell = row.GetValue(col1);

				var selectedSet = other.SelectRows(col2, SqlExpressionType.Equal, cell);

				// We've found a row in table1 that doesn't have an identical cell in
				// other, so we should include it in the result.

				if (!selectedSet.Any())
					resultRows.Add(row.Id.Number);
			}

			return resultRows;
		}

		public static IEnumerable<long> SelectRowsRange(this ITable table, int column, IndexRange[] ranges) {
			return table.GetColumnIndex(column).SelectRange(ranges);
		}

		#endregion

		public static ITable EmptySelect(this ITable table) {
			if (table.RowCount == 0)
				return table;

			return new VirtualTable(table, new long[0]);
		}

		public static Task<ITable> FullSelectAsync(this ITable table, IContext context, SqlExpression expression) {
			return TableSelects.FullSelectAsync(context, table, expression);
		}

		public static Task<ITable> SelectNonCorrelatedAsync(this ITable table, ObjectName[] columnNames, SqlExpressionType op,
			SqlExpressionType subOp, ITable other) {
			if (!op.IsQuantify())
				throw new ArgumentException();

			var isAll = op == SqlExpressionType.All;
			return TableSelects.SelectNonCorrelatedAsync(table, columnNames, subOp, isAll, other);
		}

		public static async Task<ITable> SelectNonCorrelatedAsync(this ITable table, IContext context, SqlQuantifyExpression expression) {
			return await TableSelects.QuantifiedSelectAsync(table, context, expression);
		}

		public static Task<ITable> SelectNonCorrelatedAsync(this ITable table, IContext context, ObjectName columnName,
			SqlExpressionType op, SqlExpressionType subOp, SqlExpression expression) {
			var quantified = SqlExpression.Quantify(op,
				SqlExpression.Binary(subOp, SqlExpression.Reference(columnName), expression));
			return table.SelectNonCorrelatedAsync(context, quantified);
		}

		public static async Task<ITable> SimpleSelectAsync(this ITable table, IContext context, ObjectName columnName,
			SqlExpressionType op, SqlExpression expression) {
			return await TableSelects.SimpleSelectAsync(table, context, columnName, op, expression);
		}

		public static async Task<ITable> SelectAsync(this ITable table, IContext context, SqlExpression expression) {
			if (expression is SqlQuantifyExpression) {
				return await table.SelectNonCorrelatedAsync(context, (SqlQuantifyExpression) expression);
			}
			if (expression is SqlBinaryExpression) {
				var binary = (SqlBinaryExpression) expression;
				var leftRef = binary.Left.AsReference();
				if (leftRef != null)
					return await table.SimpleSelectAsync(context, leftRef, binary.ExpressionType, binary.Right);
			}

			if (expression is SqlStringMatchExpression) {
				var stringMatch = (SqlStringMatchExpression) expression;
				var leftRef = stringMatch.Left.AsReference();
				if (leftRef != null)
					return await table.SearchAsync(context, leftRef, expression.ExpressionType, stringMatch.Pattern, stringMatch.Escape);
			}

			var value = await expression.ReduceToConstantAsync(context);
			if (value.IsNull || value.IsFalse)
				table = table.EmptySelect();

			return table;
		}

		public static async Task<ITable> SearchAsync(this ITable table, IContext context, ObjectName columnName, SqlExpressionType op, SqlExpression pattern, SqlExpression escape) {
			var column = table.TableInfo.Columns.IndexOf(columnName);
			if (column == -1)
				throw new ArgumentException();

			var patternString = await pattern.ReduceAsync(context);
			if (patternString.ExpressionType != SqlExpressionType.Constant)
				throw new InvalidOperationException();

			SqlExpression es = null;
			if (escape != null) {
				es = await escape.ReduceAsync(context);
				if (es.ExpressionType != SqlExpressionType.Constant)
					throw new InvalidOperationException();
			}

			return await TableSelects.SearchAsync(context, table, column, op, ((SqlConstantExpression) patternString).Value,
				es == null ? null : ((SqlConstantExpression) es).Value);
		}

		public static ITable SelectRange(this ITable table, ObjectName columnName, IndexRange[] ranges) {
			// If this table is empty then there is no range to select so
			// trivially return this object.
			if (table.RowCount == 0)
				return table;

			// Are we selecting a black or null range?
			if (ranges == null || ranges.Length == 0)
				// Yes, so return an empty table
				return table.EmptySelect();

			// Are we selecting the entire range?
			if (ranges.Length == 1 &&
			    ranges[0].Equals(IndexRange.FullRange))
				// Yes, so return this table.
				return table;

			// Must be a non-trivial range selection.

			// Find the column index of the column selected
			int column = table.TableInfo.Columns.IndexOf(columnName);

			if (column == -1) {
				throw new Exception(
					"Unable to find the column given to select the range of: " +
					columnName.Name);
			}

			// Select the range
			var rows = table.SelectRowsRange(column, ranges);

			// Make a new table with the range selected
			return new VirtualTable(table, rows.ToArray(), column);
		}

		public static ITable DistinctBy(this ITable table, int[] columns) {
			var resultList = new BigList<long>();
			var rowList = table.OrderRowsByColumns(columns);

			long previousRow = -1;
			foreach (var rowIndex in rowList) {
				if (previousRow != -1) {

					bool equal = true;
					// Compare cell in column in this row with previous row.
					for (int n = 0; n < columns.Length && equal; ++n) {
						var c1 = table.GetValue(rowIndex, columns[n]);
						var c2 = table.GetValue(previousRow, columns[n]);
						equal = (c1.CompareTo(c2) == 0);
					}

					if (!equal) {
						resultList.Add(rowIndex);
					}
				} else {
					resultList.Add(rowIndex);
				}

				previousRow = rowIndex;
			}

			// Return the new table with distinct rows only.
			return new VirtualTable(table, resultList);
		}

		public static ITable DistinctBy(this ITable table, ObjectName[] columnNames) {
			var mapSize = columnNames.Length;
			var map = new int[mapSize];
			for (int i = 0; i < mapSize; i++) {
				map[i] = table.TableInfo.Columns.IndexOf(columnNames[i]);
			}

			return table.DistinctBy(map);
		}


		#endregion

		#region Order By

		public static IEnumerable<long> OrderRowsByColumns(this ITable table, int[] columns) {
			var work = table.OrderBy(columns);
			// 'work' is now sorted by the columns,
			// Get the rows in this tables domain,
			var rowList = work.Select(row => row.Id.Number);

			return work.ResolveRows(0, rowList, table);
		}

		public static ITable OrderBy(this ITable table, int[] columns) {
			// Sort by the column list.
			ITable resultTable = table;
			for (int i = columns.Length - 1; i >= 0; --i) {
				resultTable = resultTable.OrderBy(columns[i], true);
			}

			// A nice post condition to check on.
			if (resultTable.RowCount != table.RowCount)
				throw new InvalidOperationException("The final row count mismatches.");

			return resultTable;
		}

		public static ITable OrderBy(this ITable table, int columnIndex, bool ascending) {
			if (table == null)
				return null;

			var rows = table.SelectAllRows(columnIndex);

			// Reverse the list if we are not ascending
			if (@ascending == false)
				rows = rows.Reverse();

			return new VirtualTable(new[] {table}, new IEnumerable<long>[] {rows});
		}

		public static ITable OrderBy(this ITable table, ObjectName columnName, bool ascending) {
			var columnOffset = table.TableInfo.Columns.IndexOf(columnName);
			if (columnOffset == -1)
				throw new ArgumentException(String.Format("Column '{0}' was not found in table.", columnName));

			return table.OrderBy(columnOffset, ascending);
		}

		public static ITable OrderBy(this ITable table, ObjectName[] columnNames, bool[] ascending) {
			var result = table;
			// Sort the results by the columns in reverse-safe order.
			int sz = ascending.Length;
			for (int n = sz - 1; n >= 0; --n) {
				result = result.OrderBy(columnNames[n], ascending[n]);
			}
			return result;
		}

		#endregion

		#region Joins

		public static ITable Outside(this ITable table, ITable rightTable) {
			// Form the row list for right hand table,
			var rowList = rightTable.Select(x => x.Id.Number).ToBigList();

			var colIndex = rightTable.TableInfo.Columns.IndexOf(table.TableInfo.Columns.GetColumnName(0));
			rowList = rightTable.ResolveRows(colIndex, rowList, table).ToBigList();

			// This row set
			var thisTableSet = table.Select(x => x.Id.Number).ToBigList();

			thisTableSet.Sort();
			rowList.Sort();

			// Find all rows that are in 'this table' and not in 'right'
			var resultList = new BigList<long>(96);
			var size = thisTableSet.Count;
			var rowListIndex = 0;
			var rowListSize = rowList.Count;
			for (long i = 0; i < size; ++i) {
				var thisVal = thisTableSet[i];
				if (rowListIndex < rowListSize) {
					var inVal = rowList[rowListIndex];
					if (thisVal < inVal) {
						resultList.Add(thisVal);
					} else if (thisVal == inVal) {
						while (rowListIndex < rowListSize &&
						       rowList[rowListIndex] == inVal) {
							++rowListIndex;
						}
					} else {
						throw new InvalidOperationException("'thisVal' > 'inVal'");
					}
				} else {
					resultList.Add(thisVal);
				}
			}

			return new VirtualTable(table, resultList);
		}

		public static Task<ITable> JoinAsync(this ITable table, IContext context, ITable other, ObjectName columnName, SqlExpressionType op, SqlExpression expression) {
			return TableJoins.JoinAsync(context, table, other, columnName, op, expression);
		}

		public static ITable NaturalJoin(this ITable table, ITable other) {
			return TableJoins.Join(table, other, true);
		}

		#endregion
	}
}