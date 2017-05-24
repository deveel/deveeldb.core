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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
				throw new DatabaseSystemException($"The default configuration format {configFormat} has no service associated");

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

		private void EnsureSystemServices() {
			var providers = scope.ResolveAll<ISystemServicesProvider>();
			foreach (var provider in providers) {
				provider.Register(scope);
			}
		}

		private string GetDatabaseName(IConfiguration config) {
			return config.GetString("database.name");
		}

		public Task StartAsync() {
			EnsureSystemServices();

			var configs = FindDatabaseConfigs();

			foreach (var config in configs) {
				var name = GetDatabaseName(config);
				var database = OpenDatabase(name, config);

				var databaseName = config.DatabaseName();
				if (String.IsNullOrWhiteSpace(databaseName))
					throw new DatabaseSystemException();

				databases[databaseName] = database;
			}

			return Task.CompletedTask;
		}

		public IEnumerable<string> GetDatabases() {
			return databases.Keys;
		}

		public IDatabase CreateDatabase(string name, IConfiguration configuration) {
			throw new NotImplementedException();
		}

		public bool DatabaseExists(string databaseName) {
			throw new NotImplementedException();
		}

		public IDatabase OpenDatabase(string name, IConfiguration configuration) {
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