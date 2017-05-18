using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public class TableCreatedEvent : TableEvent {
		public TableCreatedEvent(IEventSource source, long commitId, int tableId, ObjectName tableName) 
			: base(source, (int) 344911, commitId, tableId, tableName) {
		}
	}
}