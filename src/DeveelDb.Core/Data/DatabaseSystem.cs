﻿// 
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
		private Dictionary<string, Database> databases;
		private bool started;

		public DatabaseSystem(IConfiguration configuration) 
			: this(new ServiceContainer(), configuration) {
		}

		public DatabaseSystem(IScope scope, IConfiguration configuration) {
			this.scope = scope;
			Configuration = configuration;

			databases = new Dictionary<string, Database>();
		}

		~DatabaseSystem() {
			Dispose(false);
		}

		IEventSource IEventSource.ParentSource => null;

		IEnumerable<KeyValuePair<string, object>> IEventSource.Metadata {
			get { throw new NotImplementedException(); }
		}

		public IConfiguration Configuration { get; }

		private void ThrowIfNotStarted() {
			if (!started)
				throw new InvalidOperationException("The system was not started");
		}

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

		public async Task StartAsync() {
			EnsureSystemServices();

			var configs = FindDatabaseConfigs();

			foreach (var config in configs) {
				var database = await OpenDatabaseAsync(config);

				var databaseName = config.DatabaseName();
				if (String.IsNullOrWhiteSpace(databaseName))
					throw new DatabaseSystemException();

				databases[databaseName] = database;
			}

			started = true;
		}

		public IEnumerable<string> GetDatabases() {
			ThrowIfNotStarted();
			return databases.Keys;
		}

		public async Task<IDatabase> CreateDatabaseAsync(DatabaseBuildInfo buildInfo) {
			ThrowIfNotStarted();

			if (buildInfo == null)
				throw new ArgumentNullException(nameof(buildInfo));

			var configuration = buildInfo.Configuration;

			var name = configuration.DatabaseName();
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Database name is missing in the configuration", nameof(configuration));

			if (databases.ContainsKey(name))
				throw new InvalidOperationException($"A database named '{name}' already exists in the system context");

			var dbConfig = Configuration.MergeWith(configuration);
			var database = new Database(this, name, dbConfig);

			if (await database.ExistsAsync())
				throw new InvalidOperationException($"The database {name} already exists");

			await database.CreateAsync(buildInfo.AdminInfo);

			return database;
		}

		public Task<bool> DatabaseExistsAsync(string databaseName) {
			ThrowIfNotStarted();

			Database database;
			if (!databases.TryGetValue(databaseName, out database))
				return Task.FromResult(false);

			return database.ExistsAsync();
		}

		public async Task<Database> OpenDatabaseAsync(IConfiguration configuration) {
			ThrowIfNotStarted();

			var name = configuration.DatabaseName();
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Database name is missing in the configuration", nameof(configuration));

			var dbConfig = Configuration.MergeWith(configuration);
			var database = new Database(this, name, dbConfig);

			await database.OpenAsync();

			databases[name] = database;

			return database;
		}

		async Task<IDatabase> IDatabaseSystem.OpenDatabaseAsync(IConfiguration configuration) {
			return await OpenDatabaseAsync(configuration);
		}

		public Task<bool> DeleteDatabaseAsync(string databaseName) {
			ThrowIfNotStarted();

			Database database;
			if (!databases.TryGetValue(databaseName, out database))
				return Task.FromResult(false);

			return database.DeleteAsync();
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

			started = false;
			scope = null;
		}
	}
}