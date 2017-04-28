using System;

using Xunit;

namespace Deveel.Data.Sql.Indexes {
	public static class IndexKeyTests {
		[Theory]
		[InlineData(3L, 3L, true)]
		[InlineData(45, 32, false)]
		public static void SingleValueKeyEqual(object value1, object value2, bool expected) {
			var key1 = new IndexKey(SqlObject.New(SqlValueUtil.FromObject(value1)));
			var key2 = new IndexKey(SqlObject.New(SqlValueUtil.FromObject(value2)));

			Assert.Equal(expected, key1.Equals(key2));
		}

		[Theory]
		[InlineData(748, true, 903, true, false)]
		[InlineData(1920, 11, 1920, 11, true)]
		public static void MultiValueKeyEqual(object value1a, object value1b, object value2a, object value2b, bool expected) {
			var key1 = new IndexKey(new [] {
				SqlObject.New(SqlValueUtil.FromObject(value1a)),
				SqlObject.New(SqlValueUtil.FromObject(value1b))
			});
			var key2 = new IndexKey(new[] {
				SqlObject.New(SqlValueUtil.FromObject(value2a)),
				SqlObject.New(SqlValueUtil.FromObject(value2b))
			});

			Assert.Equal(expected, key1.Equals(key2));
		}

		[Theory]
		[InlineData(748, true)]
		public static void MultiValueKeyEqualToNull(object value1, object value2) {
			var key1 = new IndexKey(new[] {
				SqlObject.New(SqlValueUtil.FromObject(value1)),
				SqlObject.New(SqlValueUtil.FromObject(value2))
			});
			var key2 = key1.NullKey;

			Assert.NotEqual(key1, key2);
			Assert.True(key2.IsNull);
		}


		[Theory]
		[InlineData(657, 43, 1)]
		public static void CompareSingleValue(object value1, object value2, int expetced) {
			var key1 = new IndexKey(SqlObject.New(SqlValueUtil.FromObject(value1)));
			var key2 = new IndexKey(SqlObject.New(SqlValueUtil.FromObject(value2)));

			Assert.Equal(expetced, key1.CompareTo(key2));
		}

		[Theory]
		[InlineData(657, 43, 657, 11, 1)]
		public static void CompareMultipleValue(object value1a, object value1b, object value2a, object value2b, int expetced) {
			var key1 = new IndexKey(new[] {
				SqlObject.New(SqlValueUtil.FromObject(value1a)),
				SqlObject.New(SqlValueUtil.FromObject(value1b))
			});
			var key2 = new IndexKey(new[] {
				SqlObject.New(SqlValueUtil.FromObject(value2a)),
				SqlObject.New(SqlValueUtil.FromObject(value2b))
			});

			Assert.Equal(expetced, key1.CompareTo(key2));
		}
	}
}