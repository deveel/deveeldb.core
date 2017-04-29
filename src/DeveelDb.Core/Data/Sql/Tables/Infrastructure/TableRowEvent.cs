using System;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public sealed class TableRowEvent : TableEvent {
		public TableRowEvent(IEventSource source, long commitId, int tableId, ObjectName tableName, long rowNumber, TableRowEventType eventType) 
			: base(source, commitId, tableId, tableName) {
			RowNumber = rowNumber;
			EventType = eventType;
		}

		public long RowNumber { get; }

		public TableRowEventType EventType { get; }
	}
}