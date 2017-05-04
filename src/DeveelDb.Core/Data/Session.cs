using System;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Security;
using Deveel.Data.Services;
using Deveel.Data.Transactions;

namespace Deveel.Data {
	public sealed class Session : EventSource, ISession {
		private IScope scope;

		internal Session(IDatabase database, ITransaction transaction, User user, IConfiguration configuration) {
			Database = database;
			Transaction = transaction;
			scope = database.Scope.OpenScope("session");
			scope.SetConfiguration(configuration);
			scope.RegisterInstance<ISession>(this);
			User = user;
		}

		~Session() {
			Dispose(false);
		}

		public IDatabase Database { get; }

		IContext IContext.ParentContext => Database;

		string IContext.ContextName => "session";

		IScope IContext.Scope => scope;

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			
		}

		public User User { get; }

		public ITransaction Transaction { get; }

		public IQuery CreateQuery() {
			throw new NotImplementedException();
		}

		public async Task CommitAsync() {
			throw new NotImplementedException();
		}

		public async Task RollbackAsync() {
			throw new NotImplementedException();
		}
	}
}