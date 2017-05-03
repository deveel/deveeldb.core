using System;

using Deveel.Data.Configuration;

namespace Deveel.Data {
	public static class ConfigurationExtensions {
		public static string RootPath(this IConfiguration configuration) {
			return configuration.GetString("rootPath");
		}

		public static string ApplicationName(this IConfiguration configuration) {
			return configuration.GetValue<string>("applicationName");
		}

		public static string EnvironmentName(this IConfiguration configuration) {
			return configuration.GetValue<string>("environment");
		}

		public static string DatabaseName(this IConfiguration configuration) {
			return configuration.GetString("database");
		}
	}
}