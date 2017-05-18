using System.Collections.Generic;

using Deveel.Data.Diagnostics;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public class TableEvent : TransactionEvent {
		public TableEvent(IEventSource source, int eventId, long commitId, int tableId, ObjectName tableName)
			: base(source, eventId, commitId) {
			TableId = tableId;
			TableName = tableName;
		}

		public int TableId { get; }

		public ObjectName TableName { get; }

		protected override void GetEventData(Dictionary<string, object> data) {
			data["table.id"] = TableId;
			data["table.name"] = TableName.ToString();

			base.GetEventData(data);
		}
	}
}