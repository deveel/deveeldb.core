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
using System.Linq;
using System.Reflection;

using Deveel.Data.Configuration;
using Deveel.Data.Services;

using DryIoc;

using IScope = Deveel.Data.Services.IScope;

namespace Deveel.Data {
	public class SystemBuilder : ISystemBuilder {
		private List<Action<SystemBuildContext, IScope>> configurations;

		private IConfigurationBuilder settings;
		private Func<IScope> scopeFactory;

		private SystemBuildContext buildContext;

		public SystemBuilder() {
			configurations = new List<Action<SystemBuildContext, IScope>>();

			scopeFactory = () => new ServiceContainer();

			settings = new ConfigurationBuilder()
				.AddEnvironmentVariables("DEVEELDB_");

			buildContext = new SystemBuildContext {
				Settings = settings
			};
		}

		public IDatabaseSystem Build() {
			var sysConfig = settings.Build();

			var rootPath = ResolveRootPath(sysConfig.RootPath(), AppContext.BaseDirectory);
			var applicationName = sysConfig.ApplicationName();
			var envName = sysConfig.EnvironmentName();

			var environment = new SystemEnvironment {
				ApplicationName = applicationName,
				EnvironmentName = envName,
				RootPath = rootPath
			};

			buildContext.Environment = environment;

			var systemScope = scopeFactory();

			if (systemScope == null)
				throw new InvalidOperationException("The factory configured could not create a scope");

			systemScope.RegisterInstance<ISystemEnvironment>(environment);
			systemScope.RegisterInstance<SystemBuildContext>(buildContext);

			var configBuilder = new ConfigurationBuilder()
				.SetRootPath(rootPath);

			var config = configBuilder.Build();
			systemScope.RegisterInstance<IConfiguration>(config);

			foreach (var configure in configurations) {
				configure(buildContext, systemScope);
			}

			return new DatabaseSystem(systemScope, config);
		}
		
		public ISystemBuilder UseScope(Func<IScope> scope) {
			if (scope == null)
				throw new ArgumentNullException(nameof(scope));

			scopeFactory = scope;
			return this;
		}

		public ISystemBuilder UseSetting(string key, object value) {
			settings.WithSetting(key, value);
			return this;
		}

		public ISystemBuilder ConfigureServices(Action<SystemBuildContext, IScope> configure) {
			if (configure == null)
				throw new ArgumentNullException(nameof(configure));

			configurations.Add(configure);
			return this;
		}

		private static string ResolveRootPath(string rootPath, string basePath) {
			if (string.IsNullOrEmpty(rootPath))
				return basePath;
			if (Path.IsPathRooted(rootPath))
				return rootPath;

			return Path.Combine(Path.GetFullPath(basePath), rootPath);
		}
	}
}