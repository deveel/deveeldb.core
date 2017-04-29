using System;

using Deveel.Data.Services;

namespace Deveel.Data.Configuration {
	public static class ScopeExtensions {
		public static void SetConfiguration(this IScope scope, IConfiguration configuration) {
			var config = scope.Resolve<IConfiguration>();
			var final = configuration;
			if (config != null)
				final = final.MergeWith(configuration);

			scope.Unregister<IConfiguration>();
			scope.RegisterInstance<IConfiguration>(final);
		}
	}
}