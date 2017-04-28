using System;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables {
	public class TableCreatedEvent : TableEvent {
		public TableCreatedEvent(IEventSource source, long commitId, int tableId, ObjectName tableName) 
			: base(source, 344911, commitId, tableId, tableName) {
		}
	}
}