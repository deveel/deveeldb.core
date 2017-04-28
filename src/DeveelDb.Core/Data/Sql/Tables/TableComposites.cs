using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Tables {
	static class TableComposites {
		public static ITable Union(ITable thisTable, ITable otherTable) {
			// Optimizations - handle trivial case of row count in one of the tables
			//   being 0.
			// NOTE: This optimization assumes this table and the unioned table are
			//   of the same type.
			if ((thisTable.RowCount == 0 && otherTable.RowCount == 0) ||
			    otherTable.RowCount == 0) {
				return thisTable;
			}

			if (thisTable.RowCount == 0)
				return otherTable;

			// First we merge this table with the input table.

			var raw1 = thisTable.GetRawTableInfo();
			var raw2 = otherTable.GetRawTableInfo();

			// This will throw an exception if the table types do not match up.

			var union = raw1.Union(raw2);

			// Now 'union' contains a list of uniquely merged rows (ie. the union).
			// Now make it into a new table and return the information.

			var tableList = union.Tables.Cast<ITable>().ToArray();
			var rows = union.Rows.Cast<IEnumerable<long>>().ToArray();
			return new VirtualTable(tableList, rows);
		}
	}
}