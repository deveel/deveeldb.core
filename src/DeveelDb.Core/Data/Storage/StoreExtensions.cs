using System;

namespace Deveel.Data.Storage {
	public static class StoreExtensions {
		public static IArea GetArea(this IStore store, long id)
			=> store.GetArea(id, false);
	}
}