using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Services;
using Deveel.Data.Storage;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	class TableSourceComposite : ITableSourceComposite, IDisposable {
		private IStoreSystem storeSystem;
		private IStoreSystem tempStoreSystem;

		private IStore stateStore;
		private TableStateStore tableStateStore;

		private Dictionary<int, TableSource> tableSources;

		private readonly object commitLock = new object();

		public TableSourceComposite(IDatabase database) {
			Database = database;

			storeSystem = GetStoreSystem(database.Configuration);

			if (storeSystem == null)
				throw new DatabaseSystemException("No valid storage system was set");

			tempStoreSystem = new InMemoryStoreSystem();

			Init();
		}

		~TableSourceComposite() {
			Dispose(false);
		}

		public IDatabase Database { get; }

		public bool IsClosed { get; private set; }

		public long CurrentCommitId { get; private set; }

		private void Init() {
			lock (this) {
				CurrentCommitId = 0;
				tableSources = new Dictionary<int, TableSource>();
				IsClosed = false;
			}
		}

		private IStoreSystem GetStoreSystem(IConfiguration configuration) {
			var systemId = configuration.GetString("database.store.type");
			if (String.IsNullOrWhiteSpace(systemId))
				return Database.Scope.ResolveAll<IStoreSystem>().FirstOrDefault();

			return Database.GetStoreSystem(systemId);
		}

		public IStoreSystem ResolveStoreSystem(string systemId) {
			if (String.IsNullOrWhiteSpace(systemId))
				return storeSystem;

			var system = Database.GetStoreSystem(systemId);
			if (system == null)
				return storeSystem;

			return system;
		}

		async Task<ITableSource> ITableSourceComposite.CreateTableSourceAsync(TableInfo tableInfo) {
			return await CreateTableSourceAsync(tableInfo);
		}

		private Task<TableSource> CreateTableSourceAsync(TableInfo tableInfo) {
			lock (commitLock) {
				try {
					IStoreSystem tableStoreSystem;

					var temporary = tableInfo.Metadata.GetValue<bool>("temporary");
					if (temporary) {
						tableStoreSystem = tempStoreSystem;
					} else {
						var tableStoreSystemId = tableInfo.Metadata.GetValue<string>("storageSystem");
						if (String.IsNullOrWhiteSpace(tableStoreSystemId)) {
							tableStoreSystem = storeSystem;
						} else {
							tableStoreSystem = Database.Scope.Resolve<IStoreSystem>(tableStoreSystemId);

							if (tableStoreSystem == null)
								throw new ConfigurationException(
									$"The table storage system '{tableStoreSystemId}' was not found in the system");
						}
					}

					throw new NotImplementedException();
				} catch (Exception ex) {

					throw;
				}
			}
		}

		private void ReadVisibleTables() {
			lock (commitLock) {
				var tables = tableStateStore.VisibleTables;

				// For each visible table
				foreach (var resource in tables) {
					var tableId = resource.Id;
					var sourceName = resource.SourceName;

					// TODO: add a table source type?

					// Load the master table from the resource information
					var source = LoadTableSource(resource);

					if (source == null)
						throw new InvalidOperationException($"Table {sourceName} was not found in {resource.SystemId}.");

					source.Open();

					tableSources.Add(tableId, source);
				}
			}
		}

		private void ReadDroppedTables() {
			lock (commitLock) {
				// The list of all dropped tables from the state file
				var tables = tableStateStore.DeletedResources;

				// For each visible table
				foreach (var resource in tables) {
					int tableId = resource.Id;
					string tableName = resource.SourceName;

					// Load the master table from the resource information
					var source = LoadTableSource(resource);

					// File wasn't found so remove from the delete resources
					if (source == null) {
						tableStateStore.RemoveDeleteResource(tableName);
					} else {
						source.Open();

						// Add the table to the table list
						tableSources.Add(tableId, source);
					}
				}

				tableStateStore.Flush();
			}
		}



		private TableSource LoadTableSource(TableStateInfo stateInfo) {
			var tableSource = new TableSource(this, stateInfo);
			if (!tableSource.Exists())
				return null;

			return tableSource;
		}

		Task<ITableSource> ITableSourceComposite.GetTableSourceAsync(int tableId) {
			throw new NotImplementedException();
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (tempStoreSystem != null)
					tempStoreSystem.Dispose();

				if (storeSystem != null)
					storeSystem.Dispose();

			}

			tempStoreSystem = null;
			storeSystem = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}