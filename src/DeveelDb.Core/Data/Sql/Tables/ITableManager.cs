using System;

namespace Deveel.Data.Sql.Tables {
	public interface ITableManager : IDbObjectManager {
		void SelectTable(ObjectName tableName);

		ITableSource[] GetSelectedTables();

		ITableSource[] GetVisibleTables();

		IMutableTable[] GetAccessedTables();
	}
}