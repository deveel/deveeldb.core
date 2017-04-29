using System;
using System.Collections.Generic;

using Deveel.Data.Services;

namespace Deveel.Data.Sql.Variables {
	public static class ScopeExtensions {
		public static void AddVariableManager<TManager>(this IScope scope)
			where TManager : class, IVariableManager {
			scope.Register<IVariableManager, TManager>();
			scope.Register<IDbObjectManager, TManager>(DbObjectType.Variable);
			scope.Register<IDbObjectManager, TManager>();
			scope.Register<TManager>();
		}

		public static void AddVariableManager(this IScope scope) {
			scope.AddVariableManager<VariableManager>();
			scope.AddVariableResolver<VariableManager>();
		}

		public static void AddVariableResolver<TResolver>(this IScope scope)
			where TResolver : class, IVariableResolver {
			scope.Register<IVariableResolver, TResolver>();
		}

		public static void AddVariableResolver<TResolver>(this IScope scope, TResolver resolver)
			where TResolver : class, IVariableResolver {
			scope.RegisterInstance(resolver);
			scope.RegisterInstance<IVariableResolver>(resolver);
		}

		public static TManager GetVariableManager<TManager>(this IScope scope)
			where TManager : class, IVariableManager {
			return scope.Resolve<TManager>();
		}

		public static IEnumerable<IVariableResolver> GetVariableResolvers(this IScope scope) {
			return scope.ResolveAll<IVariableResolver>();
		}
	}
}