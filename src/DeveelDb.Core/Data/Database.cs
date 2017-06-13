using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Services;
using Deveel.Data.Sql.Tables.Infrastructure;
using Deveel.Data.Storage;
using Deveel.Data.Transactions;

namespace Deveel.Data {
	public sealed class Database : EventSource, IDatabase, ITransactionFactory {
		private IScope scope;
		private readonly bool ownsSystem;

		private TableSourceComposite tableComposite;

		internal Database(DatabaseSystem system, string name, IConfiguration configuration, bool ownsSystem = false) {
			System = system;
			this.ownsSystem = ownsSystem;

			scope = (system as IContext).Scope.OpenScope("database");
			scope.ReplaceInstance<IConfiguration>(configuration);
			scope.RegisterInstance<IDatabase>(this);
			scope.RegisterInstance<ITransactionFactory>(this);

			Configuration = configuration;
			Name = name;

			Version = typeof(Database).GetTypeInfo().Assembly.GetName().Version;

			tableComposite = new TableSourceComposite(this);
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

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (tableComposite != null)
					tableComposite.Dispose();

				if (scope != null)
					scope.Dispose();

				if (ownsSystem && System != null)
					System.Dispose();
			}

			scope = null;
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

		public static async Task<Database> CreateAsync(string name, IConfiguration configuration) {
			var system = new DatabaseSystem(configuration);
			return await system.CreateDatabaseAsync(name, configuration) as Database;
		}

		#endregion
	}
}