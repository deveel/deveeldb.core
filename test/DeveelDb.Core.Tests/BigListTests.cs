using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace Deveel {
	public static class BigListTests {
		[Fact]
		public static void AddRangeOf2000FromBigArray() {
			var array = new BigArray<long>(2000);
			for (int i = 0; i < 2000; i++) {
				array[i] = i * 2;
			}

			var list = new BigList<long>();
			list.AddRange(array);

			Assert.Equal(2000, list.Count);
			Assert.Equal(1999 * 2, list[list.Count - 1]);
		}

		[Fact]
		public static void AddRangeOf2000FromArray() {
			var array = new long[2000];
			for (int i = 0; i < 2000; i++) {
				array[i] = i * 2;
			}

			var list = new BigList<long>();
			list.AddRange(array);

			Assert.Equal(2000, list.Count);
			Assert.Equal(1999 * 2, list[list.Count - 1]);
		}

		[Fact]
		public static void AddRangeOf2000FromList() {
			var array = new List<long>(2000);
			for (int i = 0; i < 2000; i++) {
				array.Add(i * 2);
			}

			var list = new BigList<long>();
			list.AddRange(array);

			Assert.Equal(2000, list.Count);
			Assert.Equal(1999 * 2, list[list.Count - 1]);
		}

		[Fact]
		public static void Construct2000FromBigArray() {
			var array = new BigArray<long>(2000);
			for (int i = 0; i < 2000; i++) {
				array[i] = i * 2;
			}

			var list = new BigList<long>(array);

			Assert.Equal(2000, list.Count);
			Assert.Equal(1999 * 2, list[list.Count - 1]);
		}

		[Fact]
		public static void Construct2000FromArray() {
			var array = new long[2000];
			for (int i = 0; i < 2000; i++) {
				array[i] = i * 2;
			}

			var list = new BigList<long>(array);

			Assert.Equal(2000, list.Count);
			Assert.Equal(1999 * 2, list[list.Count - 1]);
		}
	}
}