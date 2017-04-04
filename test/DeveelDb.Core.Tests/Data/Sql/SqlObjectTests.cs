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
		[InlineData((short)3445, SqlTypeCode.Integer)]
		[InlineData((short)-4533, SqlTypeCode.Integer)]
		[InlineData(34454655344L, SqlTypeCode.BigInt)]
		[InlineData(-453377822144L, SqlTypeCode.BigInt)]
		[InlineData(223.019f, SqlTypeCode.Float)]
		[InlineData(-0.2f, SqlTypeCode.Float)]
		[InlineData(45533.94044, SqlTypeCode.Double)]
		[InlineData("the quick brown fox", SqlTypeCode.VarChar)]
		public static void NewFromObject(object value, SqlTypeCode expectedType) {
			var number = FromObject(value);
			var obj = SqlObject.New(number);

			Assert.Equal(expectedType, obj.Type.TypeCode);
			Assert.NotNull(obj.Value);
			Assert.False(obj.IsNull);
			Assert.Equal(number, obj.Value);
		}

		[Theory]
		[InlineData(2334.93f, 10.03f, false)]
		[InlineData(93044.33494003, 93044.33494003, true)]
		[InlineData("the quick brown fox", "the quick brown fox ", false)]
		[InlineData("the quick brown fox", "the quick brown fox", true)]
		[InlineData(56, 45, false)]
		public static void Operator_Equal(object value1, object value2, object expected) {
			BinaryOp((x, y) => x.Equal(y), value1, value2, expected);
		}

		[Theory]
		[InlineData(false, false, false)]
		[InlineData(true, false, true)]
		[InlineData("The quick brown Fox", "the quick brown fox", true)]
		[InlineData(9042.55f, 223.092f, true)]
		public static void Operator_NotEqual(object value1, object value2, object expected) {
			BinaryOp((x, y) => x.NotEqual(y), value1, value2, expected);
		}

		[Theory]
		[InlineData(456, 223, true)]
		[InlineData("the quick brown", "the quick brown fox", true)]
		public static void Operator_GreaterThan(object value1, object value2, object expected) {
			BinaryOp((x, y) => x.GreaterThan(y), value1, value2, expected);
		}

		private static void BinaryOp(Func<SqlObject, SqlObject, SqlObject> op, object value1, object value2, object expected) {
			var number1 = FromObject(value1);
			var number2 = FromObject(value2);

			var obj1 = SqlObject.New(number1);
			var obj2 = SqlObject.New(number2);

			var result = op(obj1, obj2);

			var expectedNumber = FromObject(expected);
			var expectedObj = SqlObject.New(expectedNumber);

			Assert.Equal(expectedObj, result);
		}


		private static ISqlValue FromObject(object value) {
			if (value == null)
				return SqlNull.Value;

			if (value is bool)
				return (SqlBoolean) (bool) value;

			if (value is byte)
				return (SqlNumber) (byte) value;
			if (value is int)
				return (SqlNumber) (int) value;
			if (value is short)
				return (SqlNumber) (short) value;
			if (value is long)
				return (SqlNumber) (long) value;
			if (value is float)
				return (SqlNumber) (float) value;
			if (value is double)
				return (SqlNumber) (double) value;

			if (value is string)
				return new SqlString((string)value);

			throw new NotSupportedException();
		}
	}
}