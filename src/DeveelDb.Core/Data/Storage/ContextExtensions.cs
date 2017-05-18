using System;

using Deveel.Data.Services;

namespace Deveel.Data.Storage {
	public static class ContextExtensions {
		public static IStoreSystem GetStoreSystem(this IContext context, string id) {
			return context.Scope.Resolve<IStoreSystem>(id);
		}
	}
}