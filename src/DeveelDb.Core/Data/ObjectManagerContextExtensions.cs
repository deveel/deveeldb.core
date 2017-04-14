using System;

using Deveel.Data.Services;

namespace Deveel.Data {
	public static class ObjectManagerContextExtensions {
		public static TManager ObjectManager<TManager>(this IContext context, DbObjectType objectType)
			where TManager : IDbObjectManager {
			var current = context;
			while (current != null) {
				if (current.Scope.IsRegistered<IDbObjectManager>(objectType))
					return (TManager) current.Scope.Resolve<IDbObjectManager>(objectType);
				if (context.Scope.IsRegistered<TManager>(objectType))
					return current.Scope.Resolve<TManager>(objectType);

				current = current.ParentContext;
			}

			return default(TManager);
		}
	}
}