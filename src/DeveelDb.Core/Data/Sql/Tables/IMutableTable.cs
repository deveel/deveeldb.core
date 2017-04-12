using System;

namespace Deveel.Data.Sql.Tables {
	public interface IMutableTable : ITable {
		RowId AddRow(Row row);

		void UpdateRow(Row row);

		void RemoveRow(RowId rowId);
	}
}