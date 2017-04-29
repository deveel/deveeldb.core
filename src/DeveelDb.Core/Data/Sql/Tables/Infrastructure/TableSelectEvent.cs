using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public class TableSelectEvent : TableEvent {
		public TableSelectEvent(IEventSource source, int eventId, long commitId, int tableId, ObjectName tableName) 
			: base(source, eventId, commitId, tableId, tableName) {
		}
	}
}