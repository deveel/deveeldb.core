using System;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public static class ContextExtensions {
		public static void TableEvent(this IContext context, int eventId, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableEvent>(eventId, commitId, tableId, tableName);

		public static void TableAccessed(this IContext context, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableAccessEvent>(commitId, tableId, tableName);

		public static void TableSelected(this IContext context, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableSelectEvent>(commitId, tableId, tableName);

		public static void TableCreated(this IContext context, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableCreatedEvent>(commitId, tableId, tableName);


	}
}