using System;

using Deveel.Data.Sql.Expressions;

using Xunit;

namespace Deveel.Data.Sql.Indexes {
	public static class IndexRangeSetTests {
		[Fact]
		public static void IntersectOnSingleKey() {
			var set = new IndexRangeSet();
			var result = set.Intersect(SqlExpressionType.Equal, new IndexKey(SqlObject.Boolean(true)));

			Assert.NotNull(result);

			var ranges = result.ToArray();

			Assert.Equal(1, ranges.Length);
			Assert.Equal(new IndexKey(SqlObject.Boolean(true)), ranges[0].StartValue);
			Assert.Equal(new IndexKey(SqlObject.Boolean(true)), ranges[0].EndValue);
		}

		[Fact]
		public static void IntersetOnTwoKeys() {
			var set = new IndexRangeSet();
			var result = set.Intersect(SqlExpressionType.LessThan, new IndexKey(SqlObject.Integer(3)));
			result = result.Intersect(SqlExpressionType.GreaterThan, new IndexKey(SqlObject.Integer(12)));

			Assert.NotNull(result);

			var ranges = result.ToArray();

			Assert.Equal(0, ranges.Length);
		}

		[Fact]
		public static void UnionTwoSets() {
			var set1 = new IndexRangeSet();
			set1.Intersect(SqlExpressionType.GreaterThan, new IndexKey(SqlObject.Integer(3)));

			var set2 = new IndexRangeSet();
			set2.Intersect(SqlExpressionType.LessThan, new IndexKey(SqlObject.Integer(12)));

			var result = set1.Union(set2);

			Assert.NotNull(result);

			var ranges = result.ToArray();

			Assert.Equal(2, ranges.Length);
		}
	}
}