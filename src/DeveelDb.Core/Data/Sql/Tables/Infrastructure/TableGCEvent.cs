using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public class TableGCEvent : TableEvent {
		public TableGCEvent(IEventSource source, int eventId, long commitId, int tableId, ObjectName tableName) 
			: base(source, eventId, commitId, tableId, tableName) {
		}
	}
}