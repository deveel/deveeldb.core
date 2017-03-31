using System;

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


		/*
		TODO:
		[Fact]
		public void Compare_BooleanToNumeric() {
			var type = PrimitiveTypes.Boolean();
			Assert.NotNull(type);

			Assert.Equal(0, type.Compare(SqlBoolean.True, SqlNumber.One));
			Assert.Equal(0, type.Compare(SqlBoolean.False, SqlNumber.Zero));
		}

		[Fact]
		public void Compare_BooleanToNumeric_Invalid() {
			var type = PrimitiveTypes.Boolean();
			Assert.NotNull(type);

			int result = -2;
			Assert.DoesNotThrow(() => result = type.Compare(SqlBoolean.True, new SqlNumber(22)));
			Assert.AreEqual(1, result);
		}

		[TestCase(SqlTypeCode.Bit, true, "1")]
		[TestCase(SqlTypeCode.Bit, false, "0")]
		[TestCase(SqlTypeCode.Boolean, true, "true")]
		[TestCase(SqlTypeCode.Boolean, false, "false")]
		public void CastToString(SqlTypeCode typeCode, bool value, string expected) {
			var type = PrimitiveTypes.Boolean(typeCode);

			var boolean = new SqlBoolean(value);

			var casted = type.CastTo(boolean, PrimitiveTypes.String());

			Assert.IsInstanceOf<SqlString>(casted);
			Assert.AreEqual(expected, casted.ToString());
		}

		[TestCase(true, 1)]
		[TestCase(false, 0)]
		public void CastToNumber(bool value, int expected) {
			var type = PrimitiveTypes.Boolean();
			var boolean = new SqlBoolean(value);

			var casted = type.CastTo(boolean, PrimitiveTypes.Numeric());

			Assert.IsInstanceOf<SqlNumber>(casted);
			Assert.AreEqual(expected, ((SqlNumber) casted).ToInt32());
		}

		[TestCase(true, 1)]
		[TestCase(false, 0)]
		public void CastToBinary(bool value, byte expected) {
			var type = PrimitiveTypes.Boolean();
			var boolean = new SqlBoolean(value);

			var casted = type.CastTo(boolean, PrimitiveTypes.Binary());

			var expectedArray = new[] {expected};

			Assert.IsInstanceOf<SqlBinary>(casted);
			Assert.AreEqual(expectedArray, ((SqlBinary) casted).ToByteArray());
		}
		*/
	}
}