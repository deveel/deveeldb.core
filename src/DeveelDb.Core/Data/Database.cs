using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Services;
using Deveel.Data.Sql.Tables;
using Deveel.Data.Sql.Tables.Infrastructure;
using Deveel.Data.Storage;
using Deveel.Data.Transactions;

namespace Deveel.Data {
	public sealed class Database : EventSource, IDatabase, ITableSourceComposite, ITransactionFactory {
		private IScope scope;
		private IStoreSystem storeSystem;
		private IStoreSystem tempStoreSystem;
		private readonly bool ownsSystem;

		internal Database(DatabaseSystem system, string name, IConfiguration configuration, bool ownsSystem = false) {
			System = system;
			this.ownsSystem = ownsSystem;

			scope = (system as IContext).Scope.OpenScope("database");
			scope.ReplaceInstance<IConfiguration>(configuration);
			scope.RegisterInstance<IDatabase>(this);
			scope.RegisterInstance<ITransactionFactory>(this);

			storeSystem = GetStoreSystem(configuration);

			if (storeSystem == null)
				throw new DatabaseSystemException("No valid storage system was set");

			tempStoreSystem = new InMemoryStoreSystem();


			Configuration = configuration;
			Name = name;

			Version = typeof(Database).GetTypeInfo().Assembly.GetName().Version;
		}

		private IStoreSystem GetStoreSystem(IConfiguration configuration) {
			var systemId = configuration.GetString("database.store.type");
			var system = scope.Resolve<IStoreSystem>(systemId);
			if (system == null)
				system = scope.ResolveAll<IStoreSystem>().FirstOrDefault(x => x.SystemId == systemId);
			if (system == null)
				system = scope.ResolveAll<IStoreSystem>().FirstOrDefault();

			return system;
		}

		~Database() {
			Dispose(false);
		}

		public Task<ITransaction> CreateTransactionAsync(IsolationLevel level) {
			throw new NotImplementedException();
		}

		IContext IContext.ParentContext => System;

		string IContext.ContextName => "database";

		IScope IContext.Scope => scope;

		Task<ITableSource> ITableSourceComposite.CreateTableSourceAsync(TableInfo tableInfo) {
			throw new NotImplementedException();
		}

		Task<ITableSource> ITableSourceComposite.GetTableSourceAsync(int tableId) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (scope != null)
					scope.Dispose();

				if (storeSystem != null)
					storeSystem.Dispose();

				if (ownsSystem && System != null)
					System.Dispose();
			}

			scope = null;
			storeSystem = null;
			System = null;
		}

		public IConfiguration Configuration { get; }

		public string Name { get; }

		public IDatabaseSystem System { get; private set; }

		public Version Version { get; }

		public bool IsOpen { get; private set; }

		public Task<bool> ExistsAsync() {
			throw new NotImplementedException();
		}

		public Task OpenAsync() {
			throw new NotImplementedException();
		}

		public Task CloseAsync() {
			throw new NotImplementedException();
		}

		#region Factory

		public static Database Create(string name, IConfiguration configuration) {
			var system = new DatabaseSystem(configuration);
			return system.CreateDatabase(name, configuration) as Database;
		}

		#endregion
	}
}