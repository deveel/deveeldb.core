using System;

namespace Deveel.Data.Storage {
	public static class ContextExtensions {
		public static IStoreSystem GetStoreSystem(this IContext context, string id) {
			return context.ResolveService<IStoreSystem>(id);
		}
	}
}