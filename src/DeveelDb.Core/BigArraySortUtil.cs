using System;
using System.Collections.Generic;

namespace Deveel {
	static class BigArraySortUtil<T> {
		private static readonly IComparer<T> comparer = new ItemComparer();

		private class ItemComparer : IComparer<T> {
			public int Compare(T x, T y) {
				if (x is IComparable<T>) {
					var a = (IComparable<T>) x;
					return a.CompareTo(y);
				} else if (x is IComparable) {
					var a = (IComparable) x;
					return a.CompareTo(y);
				} else {
					throw new NotSupportedException();
				}
			}
		}

		public static void QuickSort(BigArray<T> elements, long left, long right) {
			long i = left, j = right;
			var pivot = elements[(left + right) / 2];

			while (i <= j) {
				while (comparer.Compare(elements[i], pivot) < 0) {
					i++;
				}

				while (comparer.Compare(elements[j], pivot) > 0) {
					j--;
				}

				if (i <= j) {
					// Swap
					var tmp = elements[i];
					elements[i] = elements[j];
					elements[j] = tmp;

					i++;
					j--;
				}
			}

			// Recursive calls
			if (left < j) {
				QuickSort(elements, left, j);
			}

			if (i < right) {
				QuickSort(elements, i, right);
			}
		}
	}
}