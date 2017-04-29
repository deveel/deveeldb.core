using System;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables {
	public static class ContextExtensions {
		public static Task<bool> TableExistsAsync(this IContext context, ObjectName tableName) {
			return context.ObjectExistsAsync(DbObjectType.Table, tableName);
		}

		public static async Task<ITable> GetTableAsync(this IContext context, ObjectName tableName) {
			return (await context.GetObjectAsync(DbObjectType.Table, tableName)) as ITable;
		}

		public static async Task<TableInfo> GetTableInfoAsync(this IContext context, ObjectName tableName) {
			return (await context.GetObjectInfoAsync(DbObjectType.Table, tableName)) as TableInfo;
		}
	}
}