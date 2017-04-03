using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlBinaryTypeTests {
		[Fact]
		public static void CastToNumber() {
			var type = new SqlBinaryType(SqlTypeCode.Binary);

			var value = new SqlBinary(new byte[]{44, 95, 122, 0});
			Assert.True(type.CanCastTo(PrimitiveTypes.Numeric(22, 4)));

			var result = type.Cast(value, PrimitiveTypes.Numeric(22, 4));

			Assert.NotNull(result);
			Assert.False(result.IsNull);
			Assert.IsType<SqlNumber>(result);

			var number = (SqlNumber) result;
			Assert.NotNull(number);
			Assert.False(number.CanBeInt32);
			Assert.Equal(74445.4656, (double) number);
		}

		[Theory]
		[InlineData((byte)1, true)]
		[InlineData((byte)0, false)]
		[InlineData(null, null)]
		public static void CastToBoolean(byte? singleByte, bool? expected) {
			var type = new SqlBinaryType(SqlTypeCode.Binary);

			var value = singleByte == null ? SqlBinary.Null : new SqlBinary(new[]{singleByte.Value});
			Assert.True(type.CanCastTo(PrimitiveTypes.Bit()));

			var result = type.Cast(value, PrimitiveTypes.Bit());

			Assert.NotNull(result);
			Assert.IsType<SqlBoolean>(result);

			Assert.Equal(expected, (bool?)((SqlBoolean)result));
		}

		[Theory]
		[InlineData("the quick brown fox", SqlTypeCode.VarChar, -1, "the quick brown fox")]
		[InlineData("the quick brown fox", SqlTypeCode.Char, 10, "the quick ")]
		public static void CastToString(string s, SqlTypeCode typeCode, int maxSize, string expected) {
			var type = new SqlBinaryType(SqlTypeCode.Binary);
			var input = String.IsNullOrEmpty(s) ? SqlString.Null : new SqlString(s);
			var destType = new SqlCharacterType(typeCode, maxSize, null);

			var bytes = input.ToByteArray();
			var binary = new SqlBinary(bytes);

			Assert.True(type.CanCastTo(destType));

			var result = type.Cast(binary, destType);

			Assert.NotNull(result);
			Assert.IsType<SqlString>(result);

			Assert.Equal(expected, (SqlString)result);
		}

		[Theory]
		[InlineData("2010-02-11", SqlTypeCode.Date)]
		public static void CastToDate(string s, SqlTypeCode typeCode) {
			var type = new SqlBinaryType(SqlTypeCode.Binary);
			var date = String.IsNullOrEmpty(s) ? SqlDateTime.Null : SqlDateTime.Parse(s);
			var destType = new SqlDateTimeType(typeCode);

			var bytes = date.ToByteArray();
			var binary = new SqlBinary(bytes);

			Assert.True(type.CanCastTo(destType));

			var result = type.Cast(binary, destType);

			Assert.NotNull(result);
			Assert.IsType<SqlDateTime>(result);

			Assert.Equal(date, (SqlDateTime)result);
		}

		[Theory]
		[InlineData(SqlTypeCode.VarBinary, -1, "VARBINARY")]
		[InlineData(SqlTypeCode.Binary, 4556, "BINARY(4556)")]
		public static void GetString(SqlTypeCode typeCode, int size, string expected) {
			var type = new SqlBinaryType(typeCode, size);

			var s = type.ToString();
			Assert.Equal(expected, s);
		}

		[Theory]
		[InlineData(SqlTypeCode.Binary, -1, SqlTypeCode.Binary, -1, true)]
		[InlineData(SqlTypeCode.VarBinary, 455, SqlTypeCode.VarBinary, -1, false)]
		[InlineData(SqlTypeCode.Binary, 1024, SqlTypeCode.VarBinary, 1024, false)]
		public static void BinaryTypesEqual(SqlTypeCode typeCode1, int size1, SqlTypeCode typeCode2, int size2, bool expected) {
			var type1 = new SqlBinaryType(typeCode1, size1);
			var type2 = new SqlBinaryType(typeCode2, size2);

			Assert.Equal(expected, type1.Equals(type2));
		}

		[Fact]
		public static void BinaryTypeNotEqualToOtherType() {
			var type1 = new SqlBinaryType(SqlTypeCode.VarBinary);
			var type2 = new SqlBooleanType(SqlTypeCode.Bit);

			Assert.False(type1.Equals(type2));
		}
	}
}