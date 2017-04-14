using System;
using System.Collections.Generic;

namespace Deveel.Data.Security {
	public sealed class PrivilegesCache : ISecurityResolver, IDisposable {
		private Dictionary<Key, Privileges> cache;

		~PrivilegesCache() {
			Dispose(false);
		}

		bool ISecurityResolver.HasPrivileges(string grantee, DbObjectType objectType, ObjectName objectName, Privileges privileges) {
			Privileges userPrivileges;
			if (!TryGetPrivileges(objectType, objectName, grantee, out userPrivileges))
				return false;

			return (privileges & userPrivileges) != 0;
		}

		public bool TryGetPrivileges(DbObjectType objectType, ObjectName objectName, string grantee,
			out Privileges privileges) {
			if (cache == null) {
				privileges = Privileges.None;
				return false;
			}

			var key = new Key(objectType, objectName, grantee);
			return cache.TryGetValue(key, out privileges);
		}

		public void SetPrivileges(DbObjectType objectType, ObjectName objectName, string grantee, Privileges privileges) {
			var key = new Key(objectType, objectName, grantee);
			
			if (cache == null)
				cache = new Dictionary<Key, Privileges>();

			cache[key] = privileges;
		}

		public bool ClearPrivileges(DbObjectType objectType, ObjectName objectName, string grantee) {
			if (cache == null)
				return false;

			var key = new Key(objectType, objectName, grantee);
			return cache.Remove(key);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (cache != null)
					cache.Clear();
			}

			cache = null;
		}

		#region Key

		struct Key : IEquatable<Key> {
			private readonly DbObjectType objectType;
			private readonly ObjectName objectName;
			private readonly string grantee;

			public Key(DbObjectType objectType, ObjectName objectName, string grantee) {
				this.objectType = objectType;
				this.objectName = objectName;
				this.grantee = grantee;
			}

			public bool Equals(Key other) {
				return objectType == other.objectType &&
				       objectName.Equals(other.objectName) &&
				       String.Equals(grantee, other.grantee, StringComparison.Ordinal);
			}

			public override bool Equals(object obj) {
				if (!(obj is Key))
					return false;

				return Equals((Key)obj);
			}

			public override int GetHashCode() {
				return objectType.GetHashCode() + 
					objectName.GetHashCode() + 
					grantee.GetHashCode();
			}
		}

		#endregion
	}
}