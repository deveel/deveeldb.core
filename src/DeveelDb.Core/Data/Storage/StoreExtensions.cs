using System;
using System.IO;

namespace Deveel.Data.Storage {
	public static class StoreExtensions {
		public static IArea GetArea(this IStore store, long id) {
			return store.GetArea(id, false);
		}

		public static IArea GetReadOlyArea(this IStore store, long id) {
			return store.GetArea(id, true);
		}

		public static Stream GetAreaInputStream(this IStore store, long id) {
			return new AreaStream(store.GetArea(id, true));
		}
	}
}