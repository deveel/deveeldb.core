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


using System;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Security;
using Deveel.Data.Services;
using Deveel.Data.Sql;
using Deveel.Data.Transactions;

namespace Deveel.Data {
	public sealed class Session : EventSource, ISession {
		private IScope scope;
		private ITransaction transaction;
		private bool disposed;

		internal Session(IDatabase database, ITransaction transaction, User user, IConfiguration configuration) {
			Database = database;
			this.transaction = transaction;
			scope = database.Scope.OpenScope("session");
			scope.SetConfiguration(configuration);
			scope.RegisterInstance<ISession>(this);
			scope = scope.AsReadOnly();

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
			if (!disposed) {
				if (disposing) {
					try {
						DisposeTransaction();
					} catch (Exception ex) {
						this.Error(-2, "Error rolling back transaction at session close", ex);
					} finally {
						if (scope != null)
							scope.Dispose();
					}
				}

				transaction = null;
				scope = null;
				disposed = true;
			}
		}

		public User User { get; }

		public ITransaction Transaction {
			get {
				ThrowIfDisposed();
				return transaction;
			}
		}

		private void ThrowIfDisposed() {
			if (disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		private void DisposeTransaction() {
			if (transaction != null)
				transaction.Dispose();
		}

		public IQuery CreateQuery(SqlQuery query) {
			ThrowIfDisposed();

			throw new NotImplementedException();
		}

		public async Task CommitAsync(string savePointName) {
			ThrowIfDisposed();

			// TODO: issue a session event

			try {
				await transaction.CommitAsync(savePointName);
			} finally {
				DisposeTransaction();

				// TODO: issue a session event
			}
		}

		public async Task RollbackAsync(string savePointName) {
			ThrowIfDisposed();

			// TODO: issue a session event

			try {
				await transaction.RollbackAsync(savePointName);
			} finally {
				DisposeTransaction();

				// TODO: issue a session event
			}
		}
	}
}