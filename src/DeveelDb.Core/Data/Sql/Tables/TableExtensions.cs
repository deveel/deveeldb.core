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

using Deveel.Data.Sql.Indexes;

namespace Deveel.Data.Sql.Tables {
	public static class TableExtensions {
		internal static RawTableInfo GetRawTableInfo(this ITable table, RawTableInfo rootInfo) {
			if (table is IVirtualTable)
				return ((IVirtualTable)table).GetRawTableInfo(rootInfo);
			if (table is IRootTable)
				return ((IRootTable) table).GetRawTableInfo(rootInfo);

			throw new NotSupportedException();
		}

		internal static RawTableInfo GetRawTableInfo(this IRootTable table, RawTableInfo rootInfo) {
			var rows = table.Select(x => x.Id.Number).ToBigArray();
			rootInfo.Add(table, rows);

			return rootInfo;
		}

		internal static IEnumerable<long> ResolveRows(this ITable table, int column, IEnumerable<long> rows,
			ITable ancestor) {
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


		#region Select

		public static IEnumerable<long> SelectAllRows(this ITable table, int columnOffset) {
			return table.GetColumnIndex(columnOffset).SelectAll();
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

		#endregion
	}
}