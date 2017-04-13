using System;

using Xunit;

namespace Deveel.Data.Sql.Indexes {
	public static class IndexRangeTests {
		[Fact]
		public static void FullRangeEqual() {
			var fullRange1 = IndexRange.FullRange;
			var fullRange2 = IndexRange.FullRange;

			Assert.Equal(fullRange1, fullRange2);
		}

		[Fact]
		public static void IndexKeyNotEqualsFirstInSet() {
			var firstInSet = IndexRange.FirstInSet;
			var key = new IndexKey(new []{SqlObject.BigInt(33), SqlObject.Double(54) });

			Assert.NotEqual(firstInSet, key);
		}
	}
}