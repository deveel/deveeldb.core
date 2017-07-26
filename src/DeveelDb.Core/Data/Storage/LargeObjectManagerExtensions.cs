using System;

namespace Deveel.Data.Storage {
	public static class LargeObjectManagerExtensions {
		public static bool ReleaseObject(this ILargeObjectManager manager, int storeId, long objId)
			=> ReleaseObject(manager, new ObjectId(storeId, objId));

		public static bool ReleaseObject(this ILargeObjectManager manager, ObjectId objId) {
			var obj = manager.GetLargeObject(objId);
			if (obj == null)
				throw new InvalidOperationException($"Object with id {objId} not found: cannot release it.");

			return obj.Release();
		}
	}
}