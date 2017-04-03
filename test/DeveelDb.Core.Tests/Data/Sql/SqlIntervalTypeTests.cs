using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlIntervalTypeTests {
		[Theory]
		[InlineData("22:19:34", "2.00:00:20.444", "2.22:19:54.444")]
		[InlineData("19:11:22", null, null)]
		public static void AddDayToSecondToDayToSecond(string value1, string value2, string expected) {
			BinaryOp(type => type.Add, value1, value2, expected);
		}

		[Theory]
		[InlineData("22:19:34", "2.00:00:20.444", "-1.01:40:46.444")]
		[InlineData("19:11:22", null, null)]
		public static void SubtractDayToSecondFromDayToSecond(string value1, string value2, string expected) {
			BinaryOp(type => type.Subtract, value1, value2, expected);
		}


		private static void BinaryOp(Func<SqlType, Func<ISqlValue, ISqlValue, ISqlValue>> selector,
			string value1,
			string value2,
			string expected) {
			var type = new SqlIntervalType(SqlTypeCode.DayToSecond);
			var dts1 = String.IsNullOrEmpty(value1) ? SqlDayToSecond.Null : SqlDayToSecond.Parse(value1);
			var dts2 = String.IsNullOrEmpty(value2) ? SqlDayToSecond.Null : SqlDayToSecond.Parse(value2);

			var expectedResult = String.IsNullOrEmpty(expected) ? SqlDayToSecond.Null : SqlDayToSecond.Parse(expected);

			var op = selector(type);
			var result = op(dts1, dts2);

			Assert.Equal(expectedResult, result);
		}
	}
}