using System;

using Deveel.Data.Services;

namespace Deveel.Data.Sql.Methods {
	public static class ScopeExtensions {
		public static void AddMethodRegistry<TRegistry>(this IScope scope)
			where TRegistry : SqlMethodRegistry {
			scope.Register<IMethodResolver, TRegistry>();
			scope.Register<SqlMethodRegistry, TRegistry>();
			scope.Register<TRegistry>();
		}

		public static void AddMethodRegistry<TRegistry>(this IScope scope, TRegistry registry)
			where TRegistry : SqlMethodRegistry {
			scope.RegisterInstance<IMethodResolver>(registry);
			scope.RegisterInstance(registry);
		}
	}
}