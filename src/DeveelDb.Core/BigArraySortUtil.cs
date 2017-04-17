using System;
using System.Collections.Generic;

namespace Deveel {
	static class BigArraySortUtil<T> {
		private static readonly IComparer<T> comparer = new ItemComparer();

		private class ItemComparer : IComparer<T> {
			public int Compare(T x, T y) {
				if (x is IComparer<T>) {
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

		public static void QuickSort(BigArray<T> inputArray, long left, long right) {
			var pivotNewIndex = Partition(inputArray, left, right);
			T pivot = inputArray[(left + right) / 2];
			if (left < pivotNewIndex - 1)
				QuickSort(inputArray, left, pivotNewIndex - 1);
			if (pivotNewIndex < right)
				QuickSort(inputArray, pivotNewIndex, right);
		}

		private static long Partition(BigArray<T> inputArray, long left, long right) {
			long i = left, j = right;
			var pivot = inputArray[(left + right) / 2];

			while (i <= j) {
				while (comparer.Compare(inputArray[i], pivot) < 0)
					i++;
				while (comparer.Compare(inputArray[j], pivot) < 0)
					j--;
				if (i <= j) {
					T x = inputArray[i], y = inputArray[j];
					SwapWithTemp(ref x, ref y);
					inputArray[i] = x;
					inputArray[j] = y;
					i++; j--;
				}
			}
			return i;
		}

		private static void Swap(ref long valOne, ref long valTwo) {
			valOne = valOne + valTwo;
			valTwo = valOne - valTwo;
			valOne = valOne - valTwo;
		}

		private static void SwapWithTemp(ref T valOne, ref T valTwo) {
			var temp = valOne;
			valOne = valTwo;
			valTwo = temp;
		}
	}
}