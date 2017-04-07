using System;
using System.Collections;
using System.Linq;

using Deveel.Data.Sql.Expressions;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlArrayTests {
		[Theory]
		[InlineData(22, 45.039, "the quick brown fox")]
		public static void Create(params object[] values) {
			var array = new SqlArray(values.Select(NewExpression).ToArray());
			Assert.Equal(values.Length, array.Length);
			Assert.True(array.Length > 0);

			var expected = SqlObject.New(SqlValueUtil.FromObject(values[0]));
			var item = array[0];
			Assert.IsType<SqlConstantExpression>(item);

			var itemValue = ((SqlConstantExpression) item).Value;
			Assert.Equal(expected, itemValue);
		}

		[Theory]
		[InlineData("the quick brown fox", true, false, 22.3029)]
		public static void EnumerateItems(params object[] values) {
			var array = new SqlArray(values.Select(NewExpression).ToArray());
			Assert.Equal(values.Length, array.Length);
			Assert.True(array.Length > 0);

			var expected = SqlObject.New(SqlValueUtil.FromObject(values[0]));
			var first = array.First();
			Assert.IsType<SqlConstantExpression>(first);

			var itemValue = ((SqlConstantExpression)first).Value;
			Assert.Equal(expected, itemValue);
		}

		[Theory]
		[InlineData(5563.99, 23.33, true, false)]
		public static void GetString(params object[] values) {
			var array = new SqlArray(values.Select(NewExpression).ToArray());

			var s = array.ToString();
			Assert.NotEmpty(s);
		}

		[Theory]
		[InlineData(7383.99, true, "the quick brown fox")]
		public static void CopyTo(params object[] values) {
			var array = new SqlArray(values.Select(NewExpression).ToArray());
			var expressions = new SqlExpression[values.Length];

			array.CopyTo(expressions, 0);

			Assert.Equal(array.Length, expressions.Length);
		}

		[Fact]
		public static void InvalidListOperations() {
			var array = new SqlArray(new SqlExpression[0]);
			var list = array as IList;

			Assert.True(list.IsFixedSize);
			Assert.False(list.IsSynchronized);
			Assert.True(list.IsReadOnly);
			Assert.Equal(array.Length, list.Count);

			var dummy = SqlExpression.Constant(SqlObject.Bit(false));
			Assert.Throws<NotSupportedException>(() => list.Add(dummy));
			Assert.Throws<NotSupportedException>(() => list.Contains(dummy));
			Assert.Throws<NotSupportedException>(() => list.IndexOf(dummy));
			Assert.Throws<NotSupportedException>(() => list[0] = dummy);
			Assert.Throws<NotSupportedException>(() => list.Clear());
			Assert.Throws<NotSupportedException>(() => list.Remove(dummy));
			Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
			Assert.Throws<NotSupportedException>(() => list.Insert(2, dummy));
		}

		private static SqlExpression NewExpression(object value) {
			var sqlValue = SqlValueUtil.FromObject(value);
			var obj = SqlObject.New(sqlValue);

			return SqlExpression.Constant(obj);
		}
	}
}