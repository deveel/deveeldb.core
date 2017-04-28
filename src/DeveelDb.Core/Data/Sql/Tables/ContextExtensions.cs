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

		#region Diagnostics

		public static void TableEvent(this IContext context, int eventId, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableEvent>(eventId, commitId, tableId, tableName);

		public static void TableAccessed(this IContext context, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableAccessEvent>(commitId, tableId, tableName);

		public static void TableSelected(this IContext context, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableSelectEvent>(commitId, tableId, tableName);

		public static void TableCreated(this IContext context, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableCreatedEvent>(commitId, tableId, tableName);

		#endregion
	}
}