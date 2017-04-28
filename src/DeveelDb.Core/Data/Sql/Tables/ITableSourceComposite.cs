using System;

namespace Deveel.Data.Sql.Tables {
	public interface ITableSourceComposite {
		ITableSource CreateTableSource(TableInfo tableInfo, bool temporary);

		ITableSource GetTableSource(int tableId);
	}
}