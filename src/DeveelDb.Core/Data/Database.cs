using System;
using System.Reflection;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Services;
using Deveel.Data.Transactions;

namespace Deveel.Data {
	public sealed class Database : EventSource, IDatabase {
		private IScope scope;

		internal Database(IDatabaseSystem system, string name, IConfiguration configuration) {
			System = system;
			Name = name;
			Configuration = configuration;

			scope = system.Scope.OpenScope("database");
			scope.RegisterInstance<IDatabase>(this);
			scope.SetConfiguration(configuration);
			scope = scope.AsReadOnly();
		}

		~Database() {
			Dispose(false);
		}

		IContext IContext.ParentContext => System;

		string IContext.ContextName => "database";

		IScope IContext.Scope => scope;

		public string Name { get; }

		public IDatabaseSystem System { get; }

		public Version Version => typeof(Database).GetTypeInfo().Assembly.GetName().Version;

		public bool IsOpen { get; private set; }

		ITransactionCollection IDatabase.Transactions {
			get { throw new NotImplementedException(); }
		}

		public Task OpenAsync() {
			throw new NotImplementedException();
		}

		public Task CloseAsync() {
			throw new NotImplementedException();
		}

		public Task<bool> ExistsAsync() {
			throw new NotImplementedException();
		}

		Task<ITransaction> IDatabase.CreateTransactionAsync(IsolationLevel isolationLevel) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			
		}

		public IConfiguration Configuration { get; }
	}
}