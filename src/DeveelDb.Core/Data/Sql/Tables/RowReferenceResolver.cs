using System;

namespace Deveel.Data.Sql.Tables {
	public class RowReferenceResolver : IReferenceResolver {
		private readonly ITable table;
		private readonly long row;

		public RowReferenceResolver(ITable table, long row) {
			this.table = table;
			this.row = row;
		}

		public SqlObject ResolveReference(ObjectName referenceName) {
			var columnIndex = table.TableInfo.Columns.IndexOf(referenceName);
			if (columnIndex < 0)
				return null;

			return table.GetValue(row, columnIndex);
		}

		public SqlType ReturnType(ObjectName referenceName) {
			var columnIndex = table.TableInfo.Columns.IndexOf(referenceName);
			if (columnIndex < 0)
				return null;

			return table.TableInfo.Columns[columnIndex].ColumnType;
		}
	}
}