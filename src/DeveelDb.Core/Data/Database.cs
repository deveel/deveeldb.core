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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Security;
using Deveel.Data.Services;
using Deveel.Data.Transactions;

namespace Deveel.Data {
	public sealed class Database : EventSource, IDatabase {
		private IScope scope;
		private TransactionCollection transactions;

		internal Database(DatabaseSystem system, string name, IConfiguration configuration) {
			System = system;
			Name = name;
			Configuration = configuration;

			scope = (system as IContext).Scope.OpenScope("database");
			scope.RegisterInstance<IDatabase>(this);
			scope.SetConfiguration(configuration);
			scope = scope.AsReadOnly();

			transactions = new TransactionCollection(this);
		}

		~Database() {
			Dispose(false);
		}

		IContext IContext.ParentContext => System;

		string IContext.ContextName => "database";

		IScope IContext.Scope => scope;

		public string Name { get; }

		IDatabaseSystem IDatabase.System => System;

		public DatabaseSystem System { get; }

		public Version Version => typeof(Database).GetTypeInfo().Assembly.GetName().Version;

		public bool IsOpen { get; private set; }

		public ITransactionCollection Transactions => transactions;

		internal Task OpenAsync() {
			throw new NotImplementedException();
		}

		internal Task CloseAsync() {
			throw new NotImplementedException();
		}

		internal Task<bool> DeleteAsync() {
			throw new NotImplementedException();
		}

		internal Task CreateAsync(UserInfo adminInfo) {
			throw new NotImplementedException();
		}

		internal Task<bool> ExistsAsync() {
			if (IsOpen)
				return Task.FromResult(true);

			throw new NotImplementedException();
		}

		Task<ITransaction> IDatabase.CreateTransactionAsync(IsolationLevel isolationLevel) {
			throw new NotImplementedException();
		}

		internal void RemoveTransaction(ITransaction transaction) {
			transactions.RemoveTransaction(transaction);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			
		}

		public IConfiguration Configuration { get; }

		#region TransactionCollection

		class TransactionCollection : ITransactionCollection {
			private readonly Database database;
			private readonly List<ITransaction> transactions;
			private long minCommitId;
			private long maxCommitId;

			public TransactionCollection(Database database) {
				this.database = database;
				transactions = new List<ITransaction>();
				minCommitId = Int64.MaxValue;
				maxCommitId = 0;
			}

			public IEnumerator<ITransaction> GetEnumerator() {
				return transactions.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			public int Count {
				get {
					lock (database) {
						return transactions.Count;
					}
				}
			}

			public void AddTransaction(ITransaction transaction) {
				lock (database) {
					long currentCommitId = transaction.CommitId;
					if (currentCommitId < maxCommitId)
						throw new InvalidOperationException("Added a transaction with a lower than maximum commit id");

					transactions.Add(transaction);
					//TODO: database.NewTransaction(transaction);
					maxCommitId = currentCommitId;
				}
			}

			public void RemoveTransaction(ITransaction transaction) {
				lock (database) {
					int size = transactions.Count;
					int i = transactions.IndexOf(transaction);
					if (i == 0) {
						// First in list.
						if (i == size - 1) {
							// And last.
							minCommitId = Int32.MaxValue;
							maxCommitId = 0;
						} else {
							minCommitId = transactions[i + 1].CommitId;
						}
					} else if (i == transactions.Count - 1) {
						// Last in list.
						maxCommitId = transactions[i - 1].CommitId;
					} else if (i == -1) {
						throw new InvalidOperationException("Unable to find transaction in the list.");
					}

					transactions.RemoveAt(i);
					//TODO: database.EndTransaction(transaction);
				}
			}

			public long MinimumCommitId(ITransaction transaction) {
				lock (database) {
					long commitId = Int64.MaxValue;
					if (transactions.Count > 0) {
						// If the bottom transaction is this transaction, then go to the
						// next up from the bottom (we don't count this transaction as the
						// minimum commit_id).
						var testTransaction = transactions[0];
						if (testTransaction != transaction) {
							commitId = testTransaction.CommitId;
						} else if (transactions.Count > 1) {
							commitId = transactions[1].CommitId;
						}
					}

					return commitId;
				}
			}

			public ITransaction FindById(long commitId) {
				lock (database) {
					return transactions.FirstOrDefault(x => x.CommitId == commitId);
				}
			}
		}

		#endregion
	}
}