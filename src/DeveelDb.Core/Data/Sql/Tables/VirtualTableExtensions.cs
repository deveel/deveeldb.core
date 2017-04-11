using System;

namespace Deveel.Data.Sql.Tables {
	public static class VirtualTableExtensions {
		public static OuterTable Outer(this IVirtualTable table, IVirtualTable outside) {
			var tableInfo = table.GetRawTableInfo(new RawTableInfo());
			var baseTables = tableInfo.Tables;
			var baseRows = tableInfo.Rows;
			return new OuterTable(baseTables, baseRows, outside);
		}
	}
}