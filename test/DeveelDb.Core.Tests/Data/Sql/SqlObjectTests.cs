using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlObjectTests {
		[Theory]
		[InlineData(SqlTypeCode.Double, 9, 2, 36755.0912)]
		[InlineData(SqlTypeCode.Integer, -1, -1, 54667)]
		public static void GetNumericObject(SqlTypeCode code, int precision, int scale, double value) {
			var type = new SqlNumericType(code, precision, scale);
			var number = (SqlNumber) value;

			var obj = new SqlObject(type, number);

			Assert.Equal(type, obj.Type);
			Assert.Equal(number, obj.Value);
			Assert.False(obj.IsNull);
		}

		[Theory]
		[InlineData(SqlTypeCode.Char, 12, "hello!", "hello!      ")]
		[InlineData(SqlTypeCode.VarChar, 255, "hello!", "hello!")]
		public static void GetStringObject(SqlTypeCode code, int maxSize, string value, string expected) {
			var type = new SqlCharacterType(code, maxSize, null);
			var s = new SqlString(value);

			var obj = new SqlObject(type, s);
			Assert.Equal(type, obj.Type);
			Assert.Equal(expected, (SqlString) obj.Value);
			Assert.False(obj.IsNull);
		}

		[Theory]
		[InlineData("test1", "test1", true)]
		[InlineData("test2", "test1", false)]
		public static void StringEqualToString(string s1, string s2, bool expected) {
			var obj1 = SqlObject.String((SqlString) s1);
			var obj2 = SqlObject.String((SqlString) s2);

			var result = obj1.Equals(obj2);

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(SqlTypeCode.BigInt)]
		public static void NullObject_FromSqlNull(SqlTypeCode code) {
			var type = PrimitiveTypes.Type(code);
			var obj = new SqlObject(type, SqlNull.Value);

			Assert.Equal(code, obj.Type.TypeCode);
			Assert.Equal(type, obj.Type);
			Assert.IsType<SqlNull>(obj.Value);
		}

		[Theory]
		[InlineData(SqlTypeCode.String, SqlTypeCode.BigInt)]
		[InlineData(SqlTypeCode.Boolean, SqlTypeCode.VarChar)]
		public static void EqualNullToNull(SqlTypeCode typeCode1, SqlTypeCode typeCode2) {
			var type1 = PrimitiveTypes.Type(typeCode1);
			var type2 = PrimitiveTypes.Type(typeCode2);

			var obj1 = new SqlObject(type1, SqlNull.Value);
			var obj2 = new SqlObject(type2, SqlNull.Value);

			var result = obj1.Equal(obj2);
			var expectedResult = SqlObject.Boolean(null);

			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData(SqlTypeCode.Date, SqlTypeCode.Integer)]
		[InlineData(SqlTypeCode.Boolean, SqlTypeCode.Boolean)]
		public static void NotEqualNullToNull(SqlTypeCode typeCode1, SqlTypeCode typeCode2) {
			var type1 = PrimitiveTypes.Type(typeCode1);
			var type2 = PrimitiveTypes.Type(typeCode2);

			var obj1 = new SqlObject(type1, SqlNull.Value);
			var obj2 = new SqlObject(type2, SqlNull.Value);

			var result = obj1.NotEqual(obj2);
			var expectedResult = SqlObject.Boolean(null);

			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData(34454655, SqlTypeCode.Integer)]
		[InlineData(-45337782, SqlTypeCode.Integer)]
		public static void New_FromInt32(int value, SqlTypeCode expectedType) {
			var number = (SqlNumber) value;
			var obj = SqlObject.New(number);

			Assert.Equal(expectedType, obj.Type.TypeCode);
			Assert.NotNull(obj.Value);
			Assert.False(obj.IsNull);
			Assert.Equal(number, obj.Value);
		}

		[Theory]
		[InlineData(34454655344, SqlTypeCode.BigInt)]
		[InlineData(-453377822144, SqlTypeCode.BigInt)]
		public static void New_FromInt64(long value, SqlTypeCode expectedType) {
			var number = (SqlNumber) value;
			var obj = SqlObject.New(number);

			Assert.Equal(expectedType, obj.Type.TypeCode);
			Assert.NotNull(obj.Value);
			Assert.False(obj.IsNull);
			Assert.Equal(number, obj.Value);
		}

	}
}