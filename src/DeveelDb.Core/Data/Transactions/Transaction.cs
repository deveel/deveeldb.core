using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Services;

namespace Deveel.Data.Transactions {
	class Transaction : ITransaction {
		private IScope scope;

		public Transaction(Database database, long commitId, IsolationLevel isolationLevel) {
			// TODO: get the currently visible tables

			Database = database;
			CommitId = commitId;
			IsolationLevel = isolationLevel;

			scope = (database as IContext).Scope.OpenScope("transaction");
			// TODO: register anything here?
		}

		~Transaction() {
			Dispose(false);
		}

		IContext IContext.ParentContext => Database;

		string IContext.ContextName => "transaction";

		IScope IContext.Scope => scope;

		IConfiguration IConfigurationScope.Configuration {
			get { throw new NotImplementedException(); }
		}

		public long CommitId { get; }

		IDatabase ITransaction.Database => Database;

		public Database Database { get; }

		public IsolationLevel IsolationLevel { get; }

		public void Enter(IEnumerable<IDbObject> objects, AccessType accessType) {
			throw new NotImplementedException();
		}

		public void Exit(IEnumerable<IDbObject> objects, AccessType accessType) {
			throw new NotImplementedException();
		}

		public Task CommitAsync(string savePointName) {
			throw new NotImplementedException();
		}

		public Task RollbackAsync(string savePointName) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			
		}
	}
}