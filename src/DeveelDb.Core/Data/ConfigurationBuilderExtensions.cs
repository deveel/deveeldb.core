using System;

using Deveel.Data.Configuration;

namespace Deveel.Data {
	public static class ConfigurationBuilderExtensions {
		public static IConfigurationBuilder SetRootPath(this IConfigurationBuilder builder, string value) {
			return builder.WithSetting("rootPath", value);
		}
	}
}