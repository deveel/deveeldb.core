using System;

using Deveel.Data.Services;

namespace Deveel.Data {
	public static class ObjectManagerContextExtensions {
		public static void RegisterObjectManager<TManager>(this IContext context, DbObjectType objectType) 
			where TManager : class, IDbObjectManager {
			context.Scope.Register<IDbObjectManager, TManager>(objectType);
			context.Scope.Register<TManager>(objectType);
		}

		public static TManager ResolveObjectManager<TManager>(this IContext context, DbObjectType objectType)
			where TManager : IDbObjectManager {
			var current = context;
			while (current != null) {
				if (current.Scope.IsRegistered<IDbObjectManager>(objectType))
					return (TManager) current.Scope.Resolve<IDbObjectManager>(objectType);

				current = current.ParentContext;
			}

			return default(TManager);
		}
	}
}