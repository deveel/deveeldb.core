using System;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables.Infrastructure {
    class TableRowEvent : TableEvent {
        public TableRowEvent(IEventSource source, int eventId, long commitId, int tableId, ObjectName tableName, long rowNumber, TableRowEventType eventType) 
            : base(source, eventId, commitId, tableId, tableName) {
            RowNumber = rowNumber;
            EventType = eventType;
        }

        public long RowNumber { get; }

        public TableRowEventType EventType { get; }
    }
}