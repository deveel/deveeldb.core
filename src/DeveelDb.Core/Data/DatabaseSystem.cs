using System;
using System.Collections.Generic;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Services;

namespace Deveel.Data {
	public sealed class DatabaseSystem : EventSource, IDatabaseSystem {
		private IScope scope;

		public DatabaseSystem(IConfiguration configuration) 
			: this(new ServiceContainer(), configuration) {
		}

		public DatabaseSystem(IScope scope, IConfiguration configuration) {
			this.scope = scope;
			Configuration = configuration;
		}

		~DatabaseSystem() {
			Dispose(false);
		}

		IEventSource IEventSource.ParentSource => null;

		IEnumerable<KeyValuePair<string, object>> IEventSource.Metadata {
			get { throw new NotImplementedException(); }
		}

		public IConfiguration Configuration { get; }

		public void Start() {
			// TODO: load all databases configured

			throw new NotImplementedException();
		}

		public IEnumerable<string> GetDatabases() {
			throw new NotImplementedException();
		}

		public IDatabase CreateDatabase(IConfiguration configuration) {
			throw new NotImplementedException();
		}

		public bool DatabaseExists(string databaseName) {
			throw new NotImplementedException();
		}

		public IDatabase OpenDatabase(IConfiguration configuration) {
			throw new NotImplementedException();
		}

		public bool DeleteDatabase(string databaseName) {
			throw new NotImplementedException();
		}

		IContext IContext.ParentContext => null;

		string IContext.ContextName => "system";

		IScope IContext.Scope => scope;

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (scope != null)
					scope.Dispose();
			}

			scope = null;
		}
	}
}