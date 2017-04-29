using System;
using System.Collections.Generic;

using Deveel.Data.Diagnostics;
using Deveel.Data.Services;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public class TableEventRegistry : IEventRegistry<TableEvent> {
		private readonly ITransaction transaction;

		public TableEventRegistry(ITransaction transaction) {
			this.transaction = transaction;
		}

		public void Register(TableEvent @event) {
			var resolver = transaction.Scope.Resolve<ITableSourceResolver>();
			var source = resolver.GetTableSource(@event.TableId);

			source.EventHistory.RegisterEvent(@event);
		}

		Type IEventRegistry.EventType => typeof(TableEvent);

		void IEventRegistry.Register(IEvent @event) {
			Register((TableEvent)@event);
		}

		public void Rollback(int tableId, int count) {
			var resolver = transaction.Scope.Resolve<ITableSourceResolver>();
			var source = resolver.GetTableSource(tableId);

			source.EventHistory.Rollback(transaction, count);
		}
	}
}