using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel {
	public static class EnumerableExtensions {
		public static BigArray<T> ToBigArray<T>(this IEnumerable<T> source) {
			var size = source.LongCount();
			var array = new BigArray<T>(size);
			long index = 0;
			foreach (var item in source) {
				array[index++] = item;
			}

			return array;
		}

		public static T ElementAt<T>(this IEnumerable<T> source, long offset) {
			long index = 0;
			foreach (var item in source) {
				if (index == offset)
					return item;

				index++;
			}

			throw new ArgumentOutOfRangeException(nameof(offset), "The offset is past the enumeration size");
		}
	}
}