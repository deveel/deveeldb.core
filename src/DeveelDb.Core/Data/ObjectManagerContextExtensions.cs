using System;

using Deveel.Data.Services;

namespace Deveel.Data {
	public static class ObjectManagerContextExtensions {
		public static void RegisterObjectManager<TManager>(this IContext context, DbObjectType objectType) 
			where TManager : class, IDbObjectManager {
			context.Scope.Register<IDbObjectManager, TManager>(objectType);
			context.Scope.Register<TManager>(objectType);
		}
	}
}