using System;
using System.Globalization;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlStringTypeTests {
		[Theory]
		[InlineData(SqlTypeCode.VarChar, -1, null)]
		[InlineData(SqlTypeCode.VarChar, 255, "en-US")]
		[InlineData(SqlTypeCode.String, -1, "nb-NO")]
		[InlineData(SqlTypeCode.Char, 2, null)]
		public static void CreateStringType(SqlTypeCode typeCode, int maxSize, string locale) {
			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlStringType(typeCode, maxSize, culture);

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
			var type = new SqlStringType(typeCode, maxSize, culture);

			var sql = type.ToString();
			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("the quick brown fox", "the brown quick fox", 15)]
		[InlineData("ab12334", "kj12345", -10)]
		public static void CompareSimpleStrings(string s1, string s2, int expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			var type = new SqlStringType(SqlTypeCode.String, -1, null);
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
			var type = new SqlStringType(SqlTypeCode.String, -1, culture);

			Assert.Equal(expected, (bool) type.Greater(sqlString1, sqlString2));
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
			var type = new SqlStringType(SqlTypeCode.String, -1, culture);

			Assert.Equal(expected, (bool)type.Smaller(sqlString1, sqlString2));
		}

		[Theory]
		[InlineData("abc12345", "abc12345", null, true)]
		[InlineData("ab12345", "abc12345",  null, false)]
		[InlineData("the brown\n", "the brown", null, false)]
		public static void StringIsEqual(string s1, string s2, string locale, bool expected) {
			var sqlString1 = new SqlString(s1);
			var sqlString2 = new SqlString(s2);

			var culture = String.IsNullOrEmpty(locale) ? null : new CultureInfo(locale);
			var type = new SqlStringType(SqlTypeCode.String, -1, culture);

			Assert.Equal(expected, (bool)type.Equal(sqlString1, sqlString2));
		}

		[Theory]
		[InlineData("true", true)]
		[InlineData("FALSE", false)]
		[InlineData("TRUE", true)]
		public static void CastToBoolean(string s, bool expected) {
			var sqlString = new SqlString(s);
			var type = new SqlStringType(SqlTypeCode.String, -1, null);

			Assert.True(type.CanCastTo(PrimitiveTypes.Boolean()));
			var result = type.Cast(sqlString, PrimitiveTypes.Boolean());

			Assert.IsType<SqlBoolean>(result);
			Assert.False(result.IsNull);
			
			Assert.Equal(expected, (bool)(SqlBoolean) result);
		}

		[Theory]
		[InlineData("5628829.000021192", 5628829.000021192)]
		[InlineData("NaN", Double.NaN)]
		[InlineData("-6773.09222222", -6773.09222222)]
		[InlineData("8992e78", 8992e78)]
		public static void CastToNumber(string s, double expected) {
			var sqlString = new SqlString(s);
			var type = new SqlStringType(SqlTypeCode.String, -1, null);

			Assert.True(type.CanCastTo(PrimitiveTypes.Numeric()));
			var result = type.Cast(sqlString, PrimitiveTypes.Numeric());

			Assert.IsType<SqlNumber>(result);
			Assert.Equal(expected, ((SqlNumber) result).ToDouble());
		}

		[Theory]
		[InlineData("677110199911111", SqlTypeCode.BigInt, 677110199911111)]
		[InlineData("215", SqlTypeCode.TinyInt, 215)]
		[InlineData("71182992", SqlTypeCode.Integer, 71182992)]
		public static void CastToInteger(string s, SqlTypeCode typeCode, long expected) {
			var sqlString = new SqlString(s);
			var type = new SqlStringType(SqlTypeCode.String, -1, null);
			var destType = new SqlNumericType(typeCode, -1, -1);

			Assert.True(type.CanCastTo(destType));
			var result = type.Cast(sqlString, destType);

			Assert.IsType<SqlNumber>(result);
			Assert.Equal(expected, ((SqlNumber)result).ToInt64());
		}

		[Theory]
		[InlineData("the quick brown fox", SqlTypeCode.VarChar, 255, "the quick brown fox")]
		[InlineData("lorem ipsum dolor sit amet", SqlTypeCode.Char, 11, "lorem ipsum")]
		[InlineData("do", SqlTypeCode.Char, 10, "do        ")]
		public static void CastToString(string s, SqlTypeCode typeCode, int maxSize, string expected) {
			var sqlString = new SqlString(s);
			var type = new SqlStringType(SqlTypeCode.String, -1, null);
			var destType = new SqlStringType(typeCode, maxSize, null);

			Assert.True(type.CanCastTo(destType));
			var result = type.Cast(sqlString, destType);

			Assert.IsType<SqlString>(result);
			Assert.Equal(expected, ((SqlString)result).Value);
		}
	}
}