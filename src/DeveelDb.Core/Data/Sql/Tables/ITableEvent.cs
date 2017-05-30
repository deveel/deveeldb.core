using System;

using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables {
	public interface ITableEvent : ITransactionEvent {
		int TableId { get; }
	}
}