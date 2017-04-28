using System;
using System.Collections.Generic;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Transactions {
	public class TransactionEvent : Event {
		public TransactionEvent(IEventSource source, int eventId, long commitId) 
			: base(source, eventId) {
			CommitId = commitId;
		}

		public long CommitId { get; }

		protected override void GetEventData(Dictionary<string, object> data) {
			data["commit.id"] = CommitId;
		}
	}
}