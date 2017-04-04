using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlObjectTests {
		[Theory]
		[InlineData(SqlTypeCode.Double, 9, 2, 36755.0912)]
		[InlineData(SqlTypeCode.Integer, -1, -1, 54667)]
		public static void GetNumericObject(SqlTypeCode code, int precision, int scale, double value) {
			var type = new SqlNumericType(code, precision, scale);
			var number = (SqlNumber)value;

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
			var obj1 = SqlObject.String((SqlString)s1);
			var obj2 = SqlObject.String((SqlString)s2);

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
	}
}