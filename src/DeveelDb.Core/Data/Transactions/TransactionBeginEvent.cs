using System;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Transactions {
	public sealed class TransactionBeginEvent : TransactionEvent {
		public TransactionBeginEvent(IEventSource source, long commitId) 
			: base(source, 10012, commitId) {
		}
	}
}