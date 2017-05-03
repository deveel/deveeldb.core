using System;
using System.Reflection;

using Deveel.Data.Configuration;
using Deveel.Data.Services;

using IScope = Deveel.Data.Services.IScope;

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

		public static ISystemBuilder UseStartup(this ISystemBuilder builder, Type startupType) {
			return builder.ConfigureServices(configure => {
				if (startupType == null)
					throw new ArgumentNullException(nameof(startupType));

				if (!typeof(ISystemStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
					throw new ArgumentException($"Type '{startupType}' is not a valid system startup");

				configure.Register(typeof(ISystemStartup), startupType);
			});
		}

		public static ISystemBuilder UseStartup<TStartup>(this ISystemBuilder builder)
			where TStartup : ISystemStartup {
			return builder.UseStartup(typeof(TStartup));
		}
	}
}