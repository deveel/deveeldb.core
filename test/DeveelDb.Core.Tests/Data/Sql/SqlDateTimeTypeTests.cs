using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlDateTimeTypeTests {
		[Theory]
		[InlineData(SqlTypeCode.Time)]
		[InlineData(SqlTypeCode.TimeStamp)]
		[InlineData(SqlTypeCode.DateTime)]
		[InlineData(SqlTypeCode.Date)]
		public static void GetValidDateTimeType(SqlTypeCode typeCode) {
			var type = new SqlDateTimeType(typeCode);
			Assert.Equal(typeCode, type.TypeCode);
			Assert.True(type.IsIndexable);
			Assert.True(type.IsPrimitive);
			Assert.False(type.IsLargeObject);
			Assert.False(type.IsReference);
		}

		[Theory]
		[InlineData(SqlTypeCode.Time, "TIME")]
		[InlineData(SqlTypeCode.TimeStamp, "TIMESTAMP")]
		[InlineData(SqlTypeCode.DateTime, "DATETIME")]
		[InlineData(SqlTypeCode.Date, "DATE")]
		public static void GetString(SqlTypeCode typeCode, string expected) {
			var type = new SqlDateTimeType(typeCode);

			var s = type.ToString();
			Assert.Equal(expected, s);
		}

		[Theory]
		[InlineData("2019-01-04T02:00:30.221", SqlTypeCode.Time, "02:00:30.221")]
		[InlineData("2019-01-04T02:00:30.221", SqlTypeCode.TimeStamp, "2019-01-04T02:00:30.221")]
		[InlineData("2019-01-04T02:00:30.221", SqlTypeCode.Date, "2019-01-04")]
		public static void CastToDateTime(string s, SqlTypeCode destTypeCode, string expected) {
			var date = SqlDateTime.Parse(s);
			var type = new SqlDateTimeType(SqlTypeCode.DateTime);

			var destType = new SqlDateTimeType(destTypeCode);
			Assert.True(type.CanCastTo(date, destType));

			var result = type.Cast(date, destType);

			Assert.NotNull(result);
			Assert.IsType<SqlDateTime>(result);

			var expectedResult = SqlDateTime.Parse(expected);
			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData("2019-01-04T02:00:30.221", SqlTypeCode.DateTime, SqlTypeCode.VarChar, -1, "2019-01-04T02:00:30.221 +00:00")]
		[InlineData("2019-01-04T02:00:30.221", SqlTypeCode.DateTime, SqlTypeCode.VarChar, 150, "2019-01-04T02:00:30.221 +00:00")]
		[InlineData("2019-01-04T02:00:30.221", SqlTypeCode.DateTime, SqlTypeCode.Char, 12, "2019-01-04T0")]
		[InlineData("2019-01-04T02:00:30.221", SqlTypeCode.DateTime, SqlTypeCode.Char, 32, "2019-01-04T02:00:30.221 +00:00  ")]
		[InlineData("02:00:30.221", SqlTypeCode.Time, SqlTypeCode.Char, 32, "02:00:30.221 +00:00             ")]
		[InlineData("02:00:30.221", SqlTypeCode.Time, SqlTypeCode.Char, 7, "02:00:3")]
		public static void CastToString(string s, SqlTypeCode typeCode, SqlTypeCode destTypeCode, int maxSize, string expected) {
			var date = SqlDateTime.Parse(s);
			var type = new SqlDateTimeType(typeCode);

			var destType = new SqlCharacterType(destTypeCode, maxSize, null);
			Assert.True(type.CanCastTo(date, destType));

			var result = type.Cast(date, destType);

			Assert.NotNull(result);
			Assert.IsType<SqlString>(result);

			Assert.Equal(expected, (SqlString) result);
		}

		[Theory]
		[InlineData("2019-01-04T02:00:30.221", SqlTypeCode.Numeric, 30, 10)]
		[InlineData("2019-01-04T02:00:30.221", SqlTypeCode.BigInt, 19, 0)]
		public static void CastToNumber(string s, SqlTypeCode typeCode, int precision, int scale) {
			var date = SqlDateTime.Parse(s);
			var type = new SqlDateTimeType(SqlTypeCode.DateTime);
			var destType = new SqlNumericType(typeCode, precision, scale);

			Assert.True(type.CanCastTo(date, destType));

			var result = type.Cast(date, destType);

			Assert.NotNull(result);
			Assert.IsType<SqlNumber>(result);

			var value = (SqlNumber) result;

			var back = new SqlDateTime((long)value);

			Assert.Equal(date, back);
		}

		[Theory]
		[InlineData("2016-11-29", "10.20:00:03.445", "2016-12-09T20:00:03.445")]
		public static void Add(string date, string offset, string expected) {
			BinaryOp(type => type.Add, date, offset, expected);
		}

		[Theory]
		[InlineData("0001-02-10T00:00:01", "2.23:12:02", "0001-02-07T00:47:59")]
		public static void Subtract(string date, string offset, string expected) {
			BinaryOp(type => type.Subtract, date, offset, expected);
		}

		private static void BinaryOp(Func<SqlDateTimeType, Func<ISqlValue, ISqlValue, ISqlValue>> selector, string date, string offset, string expected) {
			var type = new SqlDateTimeType(SqlTypeCode.DateTime);
			var sqlDate = SqlDateTime.Parse(date);
			var dts = SqlDayToSecond.Parse(offset);

			var op = selector(type);
			var result = op(sqlDate, dts);

			var expectedResult = SqlDateTime.Parse(expected);

			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData("2005-04-04", 5, "2005-09-04")]
		public static void AddMonths(string date, int months, string expected) {
			BinaryOp(type => type.Add, date, months, expected);
		}

		[Theory]
		[InlineData("2013-12-01T09:11:25.893", 20, "2012-04-01T09:11:25.893")]
		public static void SubtractMonths(string date, int months, string expected) {
			BinaryOp(type => type.Subtract, date, months, expected);
		}

		private static void BinaryOp(Func<SqlDateTimeType, Func<ISqlValue, ISqlValue, ISqlValue>> selector, string date, int months, string expected)
		{
			var type = new SqlDateTimeType(SqlTypeCode.DateTime);
			var sqlDate = SqlDateTime.Parse(date);
			var ytm = new SqlYearToMonth(months);

			var op = selector(type);
			var result = op(sqlDate, ytm);

			var expectedResult = SqlDateTime.Parse(expected);

			Assert.Equal(expectedResult, result);
		}

	}
}