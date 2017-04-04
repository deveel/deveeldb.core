﻿using System;
using System.Globalization;
using System.IO;

namespace Deveel.Data.Sql {
	static class SqlStringCompare {
		public static int Compare(CultureInfo locale, ISqlString x, ISqlString y) {
			if (x == null && y == null)
				return 0;
			if (x == null && y != null)
				return 1;
			if (x != null && y == null)
				return -1;

			// If lexicographical ordering,
			if (locale == null)
				return LexicographicalOrder((ISqlString)x, (ISqlString)y);

			return locale.CompareInfo.Compare(x.ToString(), y.ToString());
		}

		private static int LexicographicalOrder(ISqlString str1, ISqlString str2) {
			// If both strings are small use the 'toString' method to compare the
			// strings.  This saves the overhead of having to store very large string
			// objects in memory for all comparisons.
			long str1Size = str1.Length;
			long str2Size = str2.Length;
			if (str1Size < 32 * 1024 &&
			    str2Size < 32 * 1024) {
				return String.Compare(str1.ToString(), str2.ToString(), StringComparison.Ordinal);
			}

			// TODO: pick one of the two encodings?

			// The minimum size
			long size = System.Math.Min(str1Size, str2Size);
			TextReader r1 = str1.GetInput();
			TextReader r2 = str2.GetInput();
			try {
				try {
					while (size > 0) {
						int c1 = r1.Read();
						int c2 = r2.Read();
						if (c1 != c2) {
							return c1 - c2;
						}
						--size;
					}
					// They compare equally up to the limit, so now compare sizes,
					if (str1Size > str2Size) {
						// If str1 is larger
						return 1;
					} else if (str1Size < str2Size) {
						// If str1 is smaller
						return -1;
					}
					// Must be equal
					return 0;
				} finally {
					r1.Dispose();
					r2.Dispose();
				}
			} catch (IOException e) {
				throw new Exception("IO Error: " + e.Message);
			}
		}
	}
}