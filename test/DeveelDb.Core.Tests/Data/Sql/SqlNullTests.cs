using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlNullTests {
		[Fact]
		public static void EqualsToOtherSqlNull() {
			var null1 = SqlNull.Value;
			var null2 = SqlNull.Value;

			Assert.True(null1.Equals(null2));
		}

		[Fact]
		public static void EqualsToNull() {
			var sqlNull = SqlNull.Value;
			Assert.True(sqlNull.Equals(null));
		}

		[Fact]
		public static void EqualsToNotNull() {
			var sqlNull = SqlNull.Value;
			Assert.False(sqlNull.Equals((SqlNumber)455));
		}

		[Fact]
		public static void GetString() {
			var sqlNull = SqlNull.Value;
			Assert.Equal("NULL", sqlNull.ToString());
		}

		[Fact]
		public static void OpEqualToNull() {
			var sqlNull = SqlNull.Value;
			Assert.True(sqlNull == null);
		}

		[Fact]
		public static void OpNotEqualToNull() {
			var sqlNull = SqlNull.Value;
			Assert.False(sqlNull != null);
		}

		[Fact]
		public static void OpEqualToSqlNull() {
			var null1 = SqlNull.Value;
			var null2 = SqlNull.Value;
			Assert.True(null1 == null2);
		}

		[Fact]
		public static void OpNotEqualToSqlNull() {
			var null1 = SqlNull.Value;
			var null2 = SqlNull.Value;
			Assert.False(null1 != null2);
		}

		[Fact]
		public static void OpEqualToObjNull() {
			var null1 = SqlNull.Value;
			var null2 = SqlNumber.Null;

			Assert.True(null1 == null2);
		}

		[Fact]
		public static void OpNotEqualToObjNull() {
			var null1 = SqlNull.Value;
			var null2 = SqlNumber.Null;

			Assert.False(null1 != null2);
		}
	}
}