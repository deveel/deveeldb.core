// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


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