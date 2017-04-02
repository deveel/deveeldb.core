using System;
using System.Net.Http.Headers;

using Xunit;

namespace Deveel.Data.Sql {
	public class SqlBooleanTypeTests {
		[Fact]
		public void Compare_Booleans() {
			var type = new SqlBooleanType(SqlTypeCode.Boolean);
			Assert.NotNull(type);

			Assert.Equal(1, type.Compare(SqlBoolean.True, SqlBoolean.False));
			Assert.Equal(-1, type.Compare(SqlBoolean.False, SqlBoolean.True));
			Assert.Equal(0, type.Compare(SqlBoolean.True, SqlBoolean.True));
			Assert.Equal(0, type.Compare(SqlBoolean.False, SqlBoolean.False));
		}

		[Fact]
		public static void Add() {
			var a = SqlBoolean.True;
			var b = SqlBoolean.False;
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var result = type.Add(a, b);

			Assert.True(result.IsNull);
			Assert.Equal(SqlBoolean.Null, result);
			Assert.Equal(SqlNull.Value, result);
		}

		[Fact]
		public static void Subtract() {
			var a = SqlBoolean.True;
			var b = SqlBoolean.False;
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var result = type.Subtract(a, b);

			Assert.True(result.IsNull);
			Assert.Equal(SqlBoolean.Null, result);
			Assert.Equal(SqlNull.Value, result);
		}

		[Fact]
		public static void Multiply() {
			var a = SqlBoolean.True;
			var b = SqlBoolean.False;
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var result = type.Multiply(a, b);

			Assert.True(result.IsNull);
			Assert.Equal(SqlBoolean.Null, result);
			Assert.Equal(SqlNull.Value, result);
		}

		[Theory]
		[InlineData(true, false, false)]
		[InlineData(true, true, true)]
		[InlineData(true, null, false)]
		public static void Equal(bool? a, bool? b, bool? expected) {
			var x = (SqlBoolean)a;
			var y = (SqlBoolean)b;
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var result = type.Equal(x, y);
			var exp = (SqlBoolean) expected;

			Assert.False(result.IsNull);
			Assert.Equal(exp, result);
		}

		[Theory]
		[InlineData(true, false, true)]
		[InlineData(true, true, false)]
		[InlineData(true, null, true)]
		public static void NotEqual(bool? a, bool? b, bool? expected) {
			var x = (SqlBoolean)a;
			var y = (SqlBoolean)b;
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var result = type.NotEqual(x, y);
			var exp = (SqlBoolean)expected;

			Assert.False(result.IsNull);
			Assert.Equal(exp, result);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		[InlineData(true, null, null)]
		public static void And(bool? a, bool? b, bool? expected) {
			var x = (SqlBoolean)a;
			var y = (SqlBoolean)b;
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var result = type.And(x, y);
			var exp = (SqlBoolean)expected;

			Assert.Equal(exp, result);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, true)]
		[InlineData(true, null, null)]
		public static void Or(bool? a, bool? b, bool? expected) {
			var x = (SqlBoolean)a;
			var y = (SqlBoolean)b;
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var result = type.Or(x, y);
			var exp = (SqlBoolean)expected;

			Assert.Equal(exp, result);
		}

		[Theory]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(null, null)]
		public static void Negate(bool? value, bool? expected) {
			var b = (SqlBoolean) value;
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var result = type.Negate(b);
			var exp = (SqlBoolean) expected;

			Assert.Equal(exp, result);
		}

		[Theory]
		[InlineData(SqlTypeCode.Bit, "BIT")]
		[InlineData(SqlTypeCode.Boolean, "BOOLEAN")]
		public static void BooleanTypeToString(SqlTypeCode typeCode, string expected) {
			var type = new SqlBooleanType(typeCode);

			var s = type.ToString();
			Assert.Equal(expected, s);
		}

		[Fact]
		public static void TrueToString() {
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var value = SqlBoolean.True;
			var s = type.ToString(value);

			Assert.Equal("TRUE", s);
		}

		[Fact]
		public static void FalseToString() {
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var value = SqlBoolean.False;
			var s = type.ToString(value);

			Assert.Equal("FALSE", s);
		}

		[Fact]
		public static void NullToString() {
			var type = new SqlBooleanType(SqlTypeCode.Boolean);

			var value = SqlBoolean.Null;
			var s = type.ToString(value);

			Assert.Equal("NULL", s);
		}


		[Fact]
		public void Compare_BooleanToNumeric_Invalid() {
			var type = PrimitiveTypes.Boolean();
			Assert.NotNull(type);
			Assert.Throws<ArgumentException>(() => type.Compare(SqlBoolean.True, new SqlNumber(22)));
		}

		[Theory]
		[InlineData(SqlTypeCode.Bit, true, "1")]
		[InlineData(SqlTypeCode.Bit, false, "0")]
		[InlineData(SqlTypeCode.Boolean, true, "TRUE")]
		[InlineData(SqlTypeCode.Boolean, false, "FALSE")]
		public void CastToString(SqlTypeCode typeCode, bool value, string expected) {
			var type = new SqlBooleanType(typeCode);

			var boolean = new SqlBoolean(value);

			var casted = type.Cast(boolean, PrimitiveTypes.String());

			Assert.IsType<SqlString>(casted);
			Assert.Equal(expected, casted.ToString());
		}


		[Theory]
		[InlineData(true, 1)]
		[InlineData(false, 0)]
		public void CastToBinary(bool value, byte expected) {
			var type = PrimitiveTypes.Boolean();
			var boolean = new SqlBoolean(value);

			var casted = type.Cast(boolean, PrimitiveTypes.Binary());

			var expectedArray = new[] {expected};

			Assert.IsType<SqlBinary>(casted);
			Assert.Equal(expectedArray, ((SqlBinary) casted).ToByteArray());
		}

		[Theory]
		[InlineData(true, 1)]
		[InlineData(false, 0)]
		public void CastToNumber(bool value, int expected) {
			var type = PrimitiveTypes.Boolean();
			var boolean = new SqlBoolean(value);

			Assert.True(type.CanCastTo(PrimitiveTypes.Numeric()));
			var casted = type.Cast(boolean, PrimitiveTypes.Numeric());

			Assert.IsType<SqlNumber>(casted);
			Assert.Equal(expected,(int) (SqlNumber) casted);
		}

		[Theory]
		[InlineData(SqlTypeCode.Bit, "BIT")]
		[InlineData(SqlTypeCode.Boolean, "BOOLEAN")]
		public void GetString(SqlTypeCode typeCode, string expected) {
			var type = new SqlBooleanType(typeCode);

			var s = type.ToString();
			Assert.Equal(expected, s);
		}

		[Theory]
		[InlineData(SqlTypeCode.Bit, SqlTypeCode.Bit, true)]
		[InlineData(SqlTypeCode.Boolean, SqlTypeCode.Boolean, true)]
		[InlineData(SqlTypeCode.Bit, SqlTypeCode.Boolean, true)]
		public void BooleanTypesEqual(SqlTypeCode typeCode1, SqlTypeCode typeCode2, bool expected) {
			var type1 = new SqlBooleanType(typeCode1);
			var type2 = new SqlBooleanType(typeCode2);

			Assert.Equal(expected, type1.Equals(type2));
		}

		[Fact]
		public void BooleanTypeNotEqualToOtherType() {
			var type1 = new SqlBooleanType(SqlTypeCode.Boolean);
			var type2 = new SqlBinaryType(SqlTypeCode.Binary);

			Assert.False(type1.Equals(type2));
		}
	}
}