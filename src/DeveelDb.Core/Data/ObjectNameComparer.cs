using System;
using System.Collections.Generic;

namespace Deveel.Data {
	public sealed class ObjectNameComparer : IEqualityComparer<ObjectName>, IComparer<ObjectName> {
		private readonly bool ignoreCase;

		public ObjectNameComparer(bool ignoreCase) {
			this.ignoreCase = ignoreCase;
		}

		public static ObjectNameComparer IgnoreCase => new ObjectNameComparer(true);

		public static ObjectNameComparer Ordinal => new ObjectNameComparer(false);

		public bool Equals(ObjectName x, ObjectName y) {
			return x.Equals(y, ignoreCase);
		}

		public int GetHashCode(ObjectName obj) {
			return obj.GetHashCode(ignoreCase);
		}

		public int Compare(ObjectName x, ObjectName y) {
			if (x == null && y == null)
				return 0;
			if (x == null)
				return -1;
			if (y == null)
				return 1;

			return x.CompareTo(y, ignoreCase);
		}
	}
}