using System;
using System.Globalization;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlCharacterTypeTests {
		[Theory]
		[InlineData(SqlTypeCode.VarChar, -1, null)]
		[InlineData(SqlTypeCode.VarChar, 255, "en-US")]
		[InlineData(SqlTypeCode.String, -1, "nb-NO")]
		[InlineData(SqlTypeCode.Char, 2, null)]
		public static void CreateStringType(SqlTypeCode typeCode, int maxSize, string locale) {
			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlCharacterType(typeCode, maxSize, culture);

			Assert.Equal(typeCode, type.TypeCode);
			Assert.Equal(maxSize, type.MaxSize);
			Assert.Equal(maxSize > 0, type.HasMaxSize);
			Assert.Equal(locale, type.Locale == null ? null : type.Locale.Name);
			Assert.True(type.IsIndexable);
			Assert.False(type.IsReference);
			Assert.False(type.IsLargeObject);
			Assert.True(type.IsPrimitive);
		}

		[Theory]
		[InlineData(SqlTypeCode.VarChar, -1, null, "VARCHAR")]
		[InlineData(SqlTypeCode.VarChar, 255, "en-US", "VARCHAR(255) COLLATE 'en-US'")]
		[InlineData(SqlTypeCode.String, -1, "nb-NO", "STRING COLLATE 'nb-NO'")]
		[InlineData(SqlTypeCode.Char, 2, null, "CHAR(2)")]
		[InlineData(SqlTypeCode.LongVarChar, -1, null, "LONG CHARACTER VARYING")]
		public static void GetTypeString(SqlTypeCode typeCode, int maxSize, string locale, string expected) {
			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlCharacterType(typeCode, maxSize, culture);

			var sql = type.ToString();
			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("the quick brown fox", "the brown quick fox", 15)]
		[InlineData("ab12334", "kj12345", -10)]
		public static void CompareSimpleStrings(string s1, string s2, int expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			var type = new SqlCharacterType(SqlTypeCode.String, -1, null);
			Assert.True(type.IsComparable(type));

			var result = type.Compare(sqlString1, sqlString2);

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("12345", "12345", null, false)]
		[InlineData("abc", "cde", null, true)]
		[InlineData("aaaaaaaabaaa", "aaaaabaaaa", null, true)]
		[InlineData("Abc", "abc", null, true)]
		[InlineData("ås", "øs", "nb-NO", true)]
		[InlineData("yolo", "yol", null, false)]
		public static void StringIsGreater(string s1, string s2, string locale, bool expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, culture);

			Assert.Equal(expected, (bool) type.Greater(sqlString1, sqlString2));
		}

		[Theory]
		[InlineData("ereee", "123bd", null, false)]
		[InlineData("abc1234", "abc1234", null, true)]
		public static void StringIsGreaterOrEqual(string s1, string s2, string locale, bool expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, culture);

			Assert.Equal(expected, (bool)type.GreaterOrEqual(sqlString1, sqlString2));
		}


		[Theory]
		[InlineData("12345", "12345", null, false)]
		[InlineData("abc", "cde", null, false)]
		[InlineData("aaaaaaaabaaa", "aaaaabaaaa", null, false)]
		[InlineData("Abc", "abc", null, false)]
		[InlineData("ås", "øs", "nb-NO", false)]
		[InlineData("yolo", "yol", null, true)]
		public static void StringIsSmaller(string s1, string s2, string locale, bool expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, culture);

			Assert.Equal(expected, (bool)type.Less(sqlString1, sqlString2));
		}

		[Theory]
		[InlineData("abc", "cde", null, false)]
		public static void StringIsSmallerOrEqual(string s1, string s2, string locale, bool expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, culture);

			Assert.Equal(expected, (bool)type.LessOrEqual(sqlString1, sqlString2));
		}

		[Theory]
		[InlineData("abc12345", "abc12345", null, true)]
		[InlineData("ab12345", "abc12345",  null, false)]
		[InlineData("the brown\n", "the brown", null, false)]
		public static void StringIsEqual(string s1, string s2, string locale, bool expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, culture);

			Assert.Equal(expected, (bool)type.Equal(sqlString1, sqlString2));
		}

		[Theory]
		[InlineData("abc12345", "abc12345", null, false)]
		[InlineData("ab12345", "abc12345", null, true)]
		public static void StringIsNotEqual(string s1, string s2, string locale, bool expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, culture);

			Assert.Equal(expected, (bool)type.NotEqual(sqlString1, sqlString2));
		}

		[Fact]
		public static void InvalidXOr() {
			InvalidOp(type => type.XOr);
		}

		[Fact]
		public static void InvalidOr() {
			InvalidOp(type => type.Or);
		}

		[Fact]
		public static void InvalidAnd() {
			InvalidOp(sqlType => sqlType.And);
		}

		[Fact]
		public static void InvalidAdd() {
			InvalidOp(sqlType => sqlType.And);
		}

		[Fact]
		public static void InvalidSubtract() {
			InvalidOp(sqlType => sqlType.Subtract);
		}

		[Fact]
		public static void InvalidMultiply() {
			InvalidOp(sqlType => sqlType.Multiply);
		}

		[Fact]
		public static void InvalidDivide() {
			InvalidOp(type => type.Divide);
		}

		[Fact]
		public static void InvalidModulo() {
			InvalidOp(type => type.Modulo);
		}

		[Fact]
		public static void InvalidNegate() {
			InvalidOp(type => type.Negate);
		}

		[Fact]
		public static void InvalidPlus() {
			InvalidOp(type => type.UnaryPlus);
		}

		private static void InvalidOp(Func<SqlType, Func<ISqlValue, ISqlValue, ISqlValue>> selector) {
			var s1 = new SqlString("ab");
			var s2 = new SqlString("cd");

			var type = new SqlCharacterType(SqlTypeCode.String, -1, null);
			var op = selector(type);
			var result = op(s1, s2);
			Assert.NotNull(result);
			Assert.IsType<SqlNull>(result);
		}

		private static void InvalidOp(Func<SqlType, Func<ISqlValue, ISqlValue>> selector) {
			var s1 = new SqlString("foo");

			var type = new SqlCharacterType(SqlTypeCode.String, -1, null);
			var op = selector(type);
			var result = op(s1);
			Assert.NotNull(result);
			Assert.IsType<SqlNull>(result);
		}



		[Theory]
		[InlineData("true", true)]
		[InlineData("FALSE", false)]
		[InlineData("TRUE", true)]
		public static void CastToBoolean(string s, bool expected) {
			var sqlString = new SqlString(s);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, null);

			Assert.True(type.CanCastTo(PrimitiveTypes.Boolean()));
			var result = type.Cast(sqlString, PrimitiveTypes.Boolean());

			Assert.IsType<SqlBoolean>(result);
			
			Assert.Equal(expected, (bool)(SqlBoolean) result);
		}

		[Theory]
		[InlineData("5628829.000021192", 5628829.000021192)]
		[InlineData("NaN", Double.NaN)]
		[InlineData("-6773.09222222", -6773.09222222)]
		[InlineData("8992e78", 8992e78)]
		public static void CastToNumber(string s, double expected) {
			var sqlString = new SqlString(s);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, null);

			Assert.True(type.CanCastTo(PrimitiveTypes.Numeric()));
			var result = type.Cast(sqlString, PrimitiveTypes.Numeric());

			Assert.IsType<SqlNumber>(result);
			Assert.Equal(expected, (double)(SqlNumber) result);
		}

		[Theory]
		[InlineData("677110199911111", SqlTypeCode.BigInt, 677110199911111)]
		[InlineData("215", SqlTypeCode.TinyInt, 215)]
		[InlineData("71182992", SqlTypeCode.Integer, 71182992)]
		public static void CastToInteger(string s, SqlTypeCode typeCode, long expected) {
			var sqlString = new SqlString(s);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, null);
			var destType = new SqlNumericType(typeCode, -1, -1);

			Assert.True(type.CanCastTo(destType));
			var result = type.Cast(sqlString, destType);

			Assert.IsType<SqlNumber>(result);
			Assert.Equal(expected, (long) (SqlNumber)result);
		}

		[Theory]
		[InlineData("the quick brown fox", SqlTypeCode.VarChar, 255, "the quick brown fox")]
		[InlineData("lorem ipsum dolor sit amet", SqlTypeCode.Char, 11, "lorem ipsum")]
		[InlineData("do", SqlTypeCode.Char, 10, "do        ")]
		public static void CastToString(string s, SqlTypeCode typeCode, int maxSize, string expected) {
			var sqlString = new SqlString(s);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, null);
			var destType = new SqlCharacterType(typeCode, maxSize, null);

			Assert.True(type.CanCastTo(destType));
			var result = type.Cast(sqlString, destType);

			Assert.IsType<SqlString>(result);
			Assert.Equal(expected, ((SqlString)result).Value);
		}

		[Theory]
		[InlineData("2011-01-11", SqlTypeCode.Date, "2011-01-11")]
		[InlineData("2014-01-21T02:10:16.908", SqlTypeCode.Date, "2014-01-21")]
		[InlineData("2014-01-21T02:10:16.908", SqlTypeCode.TimeStamp, "2014-01-21T02:10:16.908")]
		[InlineData("02:10:16.908", SqlTypeCode.Time, "02:10:16.908")]
		[InlineData("2014-01-21T02:10:16.908", SqlTypeCode.Time, "02:10:16.908")]
		public static void CastToDateTime(string s, SqlTypeCode typeCode, string expected) {
			var sqlString = new SqlString(s);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, null);
			var destType = new SqlDateTimeType(typeCode);

			Assert.True(type.CanCastTo(destType));
			var result = type.Cast(sqlString, destType);

			var expectedResult = SqlDateTime.Parse(expected);
			Assert.IsType<SqlDateTime>(result);

			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData("2.12:03:20.433", "2.12:03:20.433")]
		public static void CastToYearToMonth(string s, string expected) {
			var sqlString = new SqlString(s);
			var type = new SqlCharacterType(SqlTypeCode.String, -1, null);
			var destType = new SqlIntervalType(SqlTypeCode.DayToSecond);

			Assert.True(type.CanCastTo(destType));
			var result = type.Cast(sqlString, destType);
			Assert.IsType<SqlDayToSecond>(result);

			var expectedResult = SqlDayToSecond.Parse(expected);

			Assert.Equal(expectedResult, result);
		}
	}
}