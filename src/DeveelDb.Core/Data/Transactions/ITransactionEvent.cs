using System;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Transactions {
	public interface ITransactionEvent : IEvent {
		long CommitId { get; }
	}
}