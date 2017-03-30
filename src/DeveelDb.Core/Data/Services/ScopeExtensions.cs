using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Deveel.Data.Services {
	public static class ScopeExtensions {
		public static object Resolve(this IScope scope, Type serviceType) {
			return scope.Resolve(serviceType, null);
		}

		public static TService Resolve<TService>(this IScope scope, object serviceKey) {
			return (TService) scope.Resolve(typeof(TService), serviceKey);
		}

		public static TService Resolve<TService>(this IScope scope) {
			return scope.Resolve<TService>(null);
		}

		public static IEnumerable<TService> ResolveAll<TService>(this IScope scope) {
			if (scope == null)
				return new TService[0];

			return scope.ResolveAll(typeof (TService)).Cast<TService>();
		}
	}
}
