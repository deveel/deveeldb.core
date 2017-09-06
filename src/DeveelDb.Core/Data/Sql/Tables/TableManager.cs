using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Diagnostics;
using Deveel.Data.Sql.Sequences;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables {
	public sealed class TableManager : ITableManager {
		private List<IMutableTable> accessedTables;
		private List<ITableSource> selectedTables;
		private readonly DbObjectCache<IMutableTable> tableCache;
		private Dictionary<ObjectName, ITableSource> visibleTables;

		private List<ITableContainer> internalTables;

		public TableManager(ITransaction transaction, ITableSourceComposite composite) {
			if (transaction == null)
				throw new ArgumentNullException(nameof(transaction));

			Transaction = transaction;

			Composite = composite;

			accessedTables = new List<IMutableTable>();
			tableCache = new DbObjectCache<IMutableTable>();
			selectedTables = new List<ITableSource>();
			visibleTables = new Dictionary<ObjectName, ITableSource>(ObjectNameComparer.IgnoreCase);
			internalTables = new List<ITableContainer>();
		}

		~TableManager() {
			Dispose(false);
		}

		public ITableSourceComposite Composite { get; }

		public ITransaction Transaction { get; }

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				DisposeAccessedTables();
			}
		}

		private void DisposeAccessedTables() {
			try {
				if (accessedTables != null) {
					foreach (var table in accessedTables) {
						table.Dispose();
					}

					accessedTables.Clear();
				}
			} catch (Exception ex) {
				Transaction.Error(-1, "Error while disposing the touched tables of a transaction", ex);
			} finally {
				accessedTables = null;
			}
		}

		DbObjectType IDbObjectManager.ObjectType => DbObjectType.Table;

		Task IDbObjectManager.CreateObjectAsync(IDbObjectInfo objInfo) {
			var tableInfo = objInfo as TableInfo;
			if (tableInfo == null)
				throw new ArgumentException();

			return CreateTableAsync(tableInfo);
		}

		private async Task CreateTableAsync(TableInfo tableInfo) {
			var tableName = tableInfo.TableName;
			if (visibleTables.ContainsKey(tableName))
				throw new InvalidOperationException($"Table '{tableName}' already exists.");

			tableInfo = TableInfo.ReadOnly(tableInfo);

			var source = await Composite.CreateTableSourceAsync(tableInfo);

			// Add this table (and an index set) for this table.
			AddVisibleTable(source, source.CreateIndexSet());

			int tableId = source.TableId;
			Transaction.OnTableCreated(tableId, tableName);

			await Transaction.CreateNativeSequenceAsync(tableName);
		}

		public void SelectTable(ObjectName tableName) {
			// Special handling of internal tables,
			if (IsDynamicTable(tableName))
				return;

			ITableSource source;
			if (!visibleTables.TryGetValue(tableName, out source))
				throw new ObjectNotFoundException(tableName);

			lock (selectedTables) {
				if (!selectedTables.Contains(source))
					selectedTables.Add(source);
			}

		}

		private bool IsDynamicTable(ObjectName tableName) {
			if (internalTables.Count == 0)
				return false;

			return internalTables.Any(x => x.ContainsTable(tableName));
		}

		Task<bool> IDbObjectManager.ObjectExistsAsync(ObjectName objName) {
			throw new NotImplementedException();
		}

		Task<IDbObjectInfo> IDbObjectManager.GetObjectInfoAsync(ObjectName objectName) {
			throw new NotImplementedException();
		}

		Task<IDbObject> IDbObjectManager.GetObjectAsync(ObjectName objName) {
			throw new NotImplementedException();
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

		public ITableSource[] GetSelectedTables() {
			lock (selectedTables) {
				return selectedTables.ToArray();
			}
		}

		public ITableSource[] GetVisibleTables() {
			throw new NotImplementedException();
		}

		public IMutableTable[] GetAccessedTables() {
			return accessedTables.ToArray();
		}
	}
}