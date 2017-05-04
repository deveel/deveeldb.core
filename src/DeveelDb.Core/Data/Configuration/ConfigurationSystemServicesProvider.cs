using System;

using Deveel.Data.Services;

namespace Deveel.Data.Configuration {
	class ConfigurationSystemServicesProvider : ISystemServicesProvider {
		public void Register(IScope systemScope) {
			systemScope.Register<IConfigurationFormatter, PropertiesFormatter>("properties");
		}
	}
}