using System;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables {
	public class TableAccessEvent : TableEvent {
		public TableAccessEvent(IEventSource source, long commitId, int tableId, ObjectName tableName) 
			: base(source, 101, commitId, tableId, tableName) {
		}
	}
}