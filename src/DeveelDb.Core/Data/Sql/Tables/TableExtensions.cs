using System;
using System.Linq;

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

		public static Row GetRow(this ITable table, long row) {
			return new Row(table, row);
		}

		public static Row NewRow(this ITable table) {
			return new Row(table, -1);
		}
	}
}