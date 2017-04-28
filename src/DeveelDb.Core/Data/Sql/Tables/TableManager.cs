using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Diagnostics;
using Deveel.Data.Indexes;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables {
	public sealed class TableManager : IDbObjectManager, IEventRegistry {
		private readonly ITransaction context;
		private readonly ITableSourceComposite composite;

		private Dictionary<ObjectName, ITableSource> tableSources;
		private DbObjectCache<IMutableTable> tableCache;

		public TableManager(ITransaction context, ITableSourceComposite composite) {
			this.context = context;
			this.composite = composite;

			tableSources = new Dictionary<ObjectName, ITableSource>();
			tableCache = new DbObjectCache<IMutableTable>();
		}

		~TableManager() {
			Dispose(false);
		}

		DbObjectType IDbObjectManager.ObjectType => DbObjectType.Table;


		private async Task<IMutableTable> CreateTableAsync(ITableSource source) {
			// Create the table for this transaction.
			var table = await source.CreateTableAsync(context);

			// TODO: accessedTables.Add(table);

			context.TableAccessed(context.CommitId, source.TableId, source.TableInfo.TableName);

			return table;
		}

		Task IDbObjectManager.CreateObjectAsync(IDbObjectInfo objInfo) {
			return CreateTableAsync((TableInfo) objInfo);
		}

		public async Task CreateTableAsync(TableInfo tableInfo) {
			if (tableCache.ContainsObject(tableInfo.TableName))
				throw new ArgumentException($"The table '{tableInfo.TableName}' already exists");

			tableInfo = TableInfo.ReadOnly(tableInfo);

			var source = await composite.CreateTableSourceAsync(tableInfo);

			AddVisibleTable(source, await source.CreateIndexSetAsync());

			context.TableCreated(context.CommitId, source.TableId, source.TableInfo.TableName);
		}

		private void AddVisibleTable(ITableSource source, IIndexSet<SqlObject, long> indexSet) {
			var registry = context.ResolveService<ITableIndexSetRegistry>();
			if (registry != null)
				registry.SetTableIndexSet(source.TableInfo.TableName, indexSet);

			tableSources[source.TableInfo.TableName] = source;
		}

		Task<bool> IDbObjectManager.ObjectExistsAsync(ObjectName objName) {
			throw new NotImplementedException();
		}

		async Task<IDbObjectInfo> IDbObjectManager.GetObjectInfoAsync(ObjectName objectName) {
			return await GetTableInfoAsync(objectName);
		}

		public Task<TableInfo> GetTableInfoAsync(ObjectName tableName) {
			ITableSource source;
			if (!tableSources.TryGetValue(tableName, out source))
				return Task.FromResult<TableInfo>(null);

			return Task.FromResult(source.TableInfo);
		}

		async Task<IDbObject> IDbObjectManager.GetObjectAsync(ObjectName objName) {
			return await GetTableAsync(objName);
		}

		public async Task<IMutableTable> GetTableAsync(ObjectName tableName) {
			IMutableTable table;
			if (!tableCache.TryGetObject(tableName, out table)) {
				ITableSource source;
				if (tableSources.TryGetValue(tableName, out source)) {
					table = await CreateTableAsync(source);
					tableCache.SetObject(tableName, table);
				}
			}

			return table;
		}

		Task<bool> IDbObjectManager.AlterObjectAsync(IDbObjectInfo objInfo) {
			throw new NotImplementedException();
		}

		Task<bool> IDbObjectManager.DropObjectAsync(ObjectName objName) {
			throw new NotImplementedException();
		}

		public Task<ObjectName> ResolveNameAsync(ObjectName objName, bool ignoreCase) {
			throw new NotImplementedException();
		}

		private void Dispose(bool disposing) {
			
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		Type IEventRegistry.EventType => typeof(TransactionEvent);

		void IEventRegistry.Register(IEvent @event) {
			var te = (TransactionEvent) @event;
			if (te is TransactionBeginEvent) {
				// TODO: get all visible tables at this commit
			}
		}
	}
}