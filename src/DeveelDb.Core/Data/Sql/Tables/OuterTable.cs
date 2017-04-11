using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public class OuterTable : VirtualTable, IRootTable {
		private BigArray<long>[] outerRows;
		private long outerRowCount;

		public OuterTable(ITable[] tables, IEnumerable<long>[] rows, IVirtualTable outside)
			: base(tables, rows) {
			MergeIn(outside);
			
		}

		public override long RowCount => base.RowCount + outerRowCount;

		private void MergeIn(IVirtualTable outsideTable) {
			outerRows = new BigArray<long>[Rows.Length];
			var rawTableInfo = outsideTable.GetRawTableInfo(new RawTableInfo());

			// Get the base information,
			var baseTables = Tables;

			// The tables and rows being merged in.
			var tables = rawTableInfo.Tables;
			var rows = rawTableInfo.Rows;

			// The number of rows being merged in.
			outerRowCount = rows[0].Length;

			for (int i = 0; i < baseTables.Length; ++i) {
				var btable = baseTables[i];
				int index = -1;
				for (int n = 0; n < tables.Length && index == -1; ++n) {
					if (btable == tables[n]) {
						index = n;
					}
				}

				// If the table wasn't found, then set 'NULL' to this base_table
				if (index == -1) {
					outerRows[i] = null;
				} else {
					outerRows[i] = new BigArray<long>(outerRowCount);

					// Merge in the rows from the input table,
					var toMerge = rows[index];
					if (toMerge.Length != outerRowCount)
						throw new InvalidOperationException("Wrong size for rows being merged in.");

					for (long j = 0; j < toMerge.Length; j++) {
						outerRows[i][j] = toMerge[j];
					}
				}
			}
		}

		public override SqlObject GetValue(long row, int column) {
			int tableNum = TableInfo.GetTableOffset(column);
			var parentTable = Tables[tableNum];

			if (row >= outerRowCount) {
				row = Rows[tableNum][row - outerRowCount];
				return parentTable.GetValue(row, TableInfo.GetColumnOffset(column));
			}

			if (outerRows[tableNum] == null)
				// Special case, handling outer entries (NULL)
				return new SqlObject(TableInfo.Columns[column].ColumnType, null);

			row = outerRows[tableNum][row];
			return parentTable.GetValue(row, TableInfo.GetColumnOffset(column));
		}
	}
}