using System;

namespace Deveel.Data.Sql.Tables {
	public static class TableFieldCacheExtensions {
		public static bool Remove(this ITableFieldCache cache, int tableId, int column, long row) {
			return cache.Remove(new FieldId(new RowId(tableId, row), column));
		}
	}
}