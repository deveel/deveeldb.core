using System;
using System.Collections.Generic;

namespace Deveel.Data {
	public class DbObjectCache<TCached> : IDisposable where TCached : class, IDbObject {
		private IDictionary<ObjectName, ObjectName> ignoreCaseNames;
		private IDictionary<ObjectName, TCached> cache;
		
		public DbObjectCache() {
			ignoreCaseNames = new Dictionary<ObjectName, ObjectName>(ObjectNameComparer.IgnoreCase);
			cache = new Dictionary<ObjectName, TCached>(ObjectNameComparer.Ordinal);
		}

		~DbObjectCache() {
			Dispose(false);
		}

		public bool TryResolveName(ObjectName name, bool ignoreCase, out ObjectName resolved) {
			if (ignoreCase)
				return ignoreCaseNames.TryGetValue(name, out resolved);

			if (cache.ContainsKey(name)) {
				resolved = name;
				return true;
			}

			resolved = null;
			return false;
		}

		public void SetObject(TCached cached) {
			if (cached == null)
				throw new ArgumentNullException(nameof(cached));

			SetObject(cached.ObjectInfo.FullName, cached);
		}

		public void SetObject(ObjectName name, TCached obj) {
			ignoreCaseNames[name] = name;
			cache[name] = obj;
		}

		public bool TryGetObject(ObjectName name, out TCached obj) {
			return cache.TryGetValue(name, out obj);
		}

		public bool ContainsObject(ObjectName name) {
			return cache.ContainsKey(name);
		}

		public bool RemoveObject(ObjectName name) {
			return ignoreCaseNames.Remove(name) && cache.Remove(name);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (ignoreCaseNames != null)
					ignoreCaseNames.Clear();
				if (cache != null)
					cache.Clear();
			}

			cache = null;
			ignoreCaseNames = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}