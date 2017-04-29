using System;

using Deveel.Data.Services;

namespace Deveel.Data {
	public static class ScopeExtensions {
		public static void AddObjectManager<TManager>(this IScope scope, DbObjectType objectType)
			where TManager : class, IDbObjectManager {
			scope.Register<IDbObjectManager, TManager>(objectType);
			scope.Register<IDbObjectManager, TManager>();
			scope.Register<TManager>();
		}

		public static void AddObjectManager<TManager>(this IScope scope, TManager manager)
			where TManager : class, IDbObjectManager {
			scope.RegisterInstance<IDbObjectManager>(manager);
			scope.RegisterInstance<IDbObjectManager>(manager, manager.ObjectType);
		}
	}
}