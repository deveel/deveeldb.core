using System;
using System.Collections.Generic;
using System.IO;

using Deveel.Data.Configuration;
using Deveel.Data.Diagnostics;
using Deveel.Data.Services;

namespace Deveel.Data {
	public sealed class DatabaseSystem : EventSource, IDatabaseSystem {
		private IScope scope;
		private Dictionary<string, IDatabase> databases;

		public DatabaseSystem(IConfiguration configuration) 
			: this(new ServiceContainer(), configuration) {
		}

		public DatabaseSystem(IScope scope, IConfiguration configuration) {
			this.scope = scope;
			Configuration = configuration;

			databases = new Dictionary<string, IDatabase>();
		}

		~DatabaseSystem() {
			Dispose(false);
		}

		IEventSource IEventSource.ParentSource => null;

		IEnumerable<KeyValuePair<string, object>> IEventSource.Metadata {
			get { throw new NotImplementedException(); }
		}

		public IConfiguration Configuration { get; }

		private IEnumerable<IConfiguration> FindDatabaseConfigs() {
			var basePath = Configuration.RootPath();
			var paths = Directory.GetDirectories(basePath);

			var configFileName = Configuration.GetString("database.config.fileName", "db.config");
			var configFileExt = Configuration.GetString("database.config.fileExtension");
			var configFormat = Configuration.GetString("database.config.fileFormat", "properties");

			var formatter = scope.Resolve<IConfigurationFormatter>(configFormat);
			if (formatter == null)
				throw new DatabaseSystemException();

			if (!String.IsNullOrWhiteSpace(configFileExt))
				configFileName = $"{configFileName}.{configFileExt}";

			foreach (var path in paths) {
				var fileName = Path.Combine(path, configFileName);

				var builder = new ConfigurationBuilder()
					.Add(Configuration)
					.AddFile(fileName, formatter);

				yield return builder.Build();
			}
		}

		public void Start() {
			var configs = FindDatabaseConfigs();

			foreach (var config in configs) {
				var database = OpenDatabase(config);

				var databaseName = config.DatabaseName();
				if (String.IsNullOrWhiteSpace(databaseName))
					throw new DatabaseSystemException();

				databases[databaseName] = database;
			}
		}

		public IEnumerable<string> GetDatabases() {
			return databases.Keys;
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