using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlDayToSecondTypeTests {
		[Theory]
		[InlineData("22:19:34", "2.00:00:20.444", "2.22:19:54.444")]
		public static void AddDayToSecond(string value1, string value2, string expected) {
			Binary(type => type.Add, value1, value2, expected);
		}

		[Theory]
		[InlineData("22:19:34", "2.00:00:20.444", "-1.01:40:46.444")]
		public static void SubtractDayToSecond(string value1, string value2, string expected) {
			Binary(type => type.Subtract, value1, value2, expected);
		}

		private static void Binary(Func<SqlType, Func<ISqlValue, ISqlValue, ISqlValue>> selector,
			string value1,
			string value2,
			string expected) {
			var type = new SqlDayToSecondType();

			var a = SqlDayToSecond.Parse(value1);
			var b = SqlDayToSecond.Parse(value2);

			var op = selector(type);
			var result = op(a, b);

			var exp = SqlDayToSecond.Parse(expected);

			Assert.Equal(exp, result);
		}
	}
}