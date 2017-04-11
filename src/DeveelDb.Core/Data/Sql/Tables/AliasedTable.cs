using System;

namespace Deveel.Data.Sql.Tables {
	public class AliasedTable : FilterTable, IRootTable {
		public AliasedTable(ITable table, ObjectName alias)
			: base(table) {
			TableInfo = TableInfo.Alias(table.TableInfo, alias);
		}

		public override TableInfo TableInfo { get; }
	}
}