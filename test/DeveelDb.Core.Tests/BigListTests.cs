using System;
using System.Linq;

using Xunit;

namespace Deveel {
	public static class BigListTests {
		[Fact]
		public static void AddCollection() {
			var list = new BigList<long>();
			list.AddRange(new long[]{45, 901, 11022, 542211});

			Assert.Equal(4, list.Count);
			Assert.Equal(4, list.Capacity);

			Assert.Equal(45, list[0]);
			Assert.Equal(901, list[1]);
			Assert.Equal(11022, list[2]);
			Assert.Equal(542211, list[3]);
		}

		[Fact]
		public static void IndexOf() {
			var list = new BigList<long>(1024);

			list.Add(9112);

			Assert.Equal(1024, list.Capacity);
			Assert.Equal(1, list.Count);

			var index = list.IndexOf(9112);
			Assert.Equal(0, index);
		}

		[Fact]
		public static void Enumerate() {
			var list = new BigList<long>(new long[] { 45, 901, 11022, 542211, 912113 });
			var count = list.Count();
			Assert.Equal(5, count);

			var last = list.Last();
			Assert.Equal(912113, last);
		}

		[Fact]
		public static void Remove() {
			var list = new BigList<long>(new long[] { 45, 901, 11022, 542211, 912113 });

			Assert.True(list.Remove(542211));
			Assert.Equal(4, list.Count);
		}

		[Fact]
		public static void RemoveAt() {
			var list = new BigList<long>(new long[] { 45, 901, 11022, 542211, 912113 });

			list.RemoveAt(3);
			Assert.Equal(4, list.Count);
		}
	}
}