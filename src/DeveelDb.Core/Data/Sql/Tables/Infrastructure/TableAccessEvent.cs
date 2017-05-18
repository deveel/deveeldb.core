using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public class TableAccessEvent : TableEvent {
		public TableAccessEvent(IEventSource source, long commitId, int tableId, ObjectName tableName) 
			: base(source, (int) 101, commitId, tableId, tableName) {
		}
	}
}