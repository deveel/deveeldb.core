using System;

using Deveel.Data.Configuration;
using Deveel.Data.Services;

namespace Deveel.Data {
	public static class SystemBuilderExtensions {
		public static ISystemBuilder ConfigureServices(this ISystemBuilder builder, Action<IScope> configure) {
			return builder.ConfigureServices((context, scope) => configure(scope));
		}

		public static ISystemBuilder UseConfiguration(this ISystemBuilder builder, IConfiguration configuration) {
			return builder.ConfigureServices(services => services.ReplaceInstance<IConfiguration>(configuration));
		}

		public static ISystemBuilder UseRootPath(this ISystemBuilder builder, string path) {
			return builder.UseSetting("rootPath", path);
		}
	}
}