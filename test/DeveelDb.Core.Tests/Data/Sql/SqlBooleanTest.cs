using System;
using System.ComponentModel;
using System.Globalization;

using Xunit;

namespace Deveel.Data.Sql {
	public class SqlBooleanTest {
		[Theory]
		[InlineData(1, true)]
		[InlineData(0, false)]
		public void CreateFromByte(byte value, bool expected) {
			var b = new SqlBoolean(value);
			var expectedResult = (SqlBoolean) expected;
			Assert.Equal(expectedResult,b);
		}

		[Fact]
		public void CreateFromBoolean() {
			var value = new SqlBoolean(true);
			Assert.NotNull(value);
			Assert.Equal(true, (bool)value);

			value = new SqlBoolean(false);
			Assert.NotNull(value);
			Assert.Equal(false, (bool)value);
		}

		[Fact]
		public void Compare_Equal() {
			var value1 = SqlBoolean.True;
			var value2 = new SqlBoolean(true);

			Assert.NotNull(value1);
			Assert.NotNull(value2);

			Assert.True((value1 as ISqlValue).IsComparableTo(value2));

			int i = -2;
			i = value1.CompareTo(value2);
			Assert.Equal(0, i);
		}

		[Fact]
		public void Compare_NotEqual() {
			var value1 = SqlBoolean.False;
			var value2 = new SqlBoolean(true);

			Assert.NotNull(value1);
			Assert.NotNull(value2);

			Assert.True((value1 as ISqlValue).IsComparableTo(value2));

			int i = -2;
			i = value1.CompareTo(value2);
			Assert.Equal(-1, i);
		}

		[Fact]
		public void Compare_ToNull() {
			var value1 = SqlBoolean.True;
			var value2 = SqlNull.Value;

			Assert.NotNull(value1);
			Assert.True(value2.IsNull);

			Assert.False((value1 as ISqlValue).IsComparableTo(value2));
			Assert.Throws<ArgumentException>(() => value1.CompareTo(value2));
		}

		[Fact]
		public void Compare_ToNumber_InRange() {
			var value1 = SqlBoolean.True;
			var value2 = SqlNumber.One;

			Assert.NotNull(value1);
			Assert.NotNull(value2);

			Assert.True((value1 as ISqlValue).IsComparableTo(value2));

			int i = -2;
			i = value1.CompareTo(value2);
			Assert.Equal(0, i);

			value2 = SqlNumber.Zero;

			Assert.NotNull(value1);
			Assert.NotNull(value2);

			Assert.True((value1 as ISqlValue).IsComparableTo(value2));

			i = -2;
			i = value1.CompareTo(value2);
			Assert.Equal(1, i);
		}

		[Fact]
		public void Compare_ToNumber_OutOfRange() {
			var value1 = SqlBoolean.True;
			var value2 = (SqlNumber)21;

			Assert.NotNull(value1);
			Assert.NotNull(value2);

			Assert.False((value1 as ISqlValue).IsComparableTo(value2));

			int i = -2;
			Assert.Throws<ArgumentOutOfRangeException>(() => i = value1.CompareTo(value2));
			Assert.Equal(-2, i);
		}

		[Fact]
		public void Equality_True() {
			var value1 = SqlBoolean.True;
			var value2 = SqlBoolean.True;

			Assert.True(value1 == value2);
		}

		[Fact]
		public void Equality_False() {
			var value1 = SqlBoolean.True;
			var value2 = SqlBoolean.False;

			Assert.True(value1 != value2);
		}

		[Theory]
		[InlineData(true, typeof(bool), true)]
		[InlineData(false, typeof(bool), false)]
		[InlineData(true, typeof(string), "true")]
		[InlineData(false, typeof(string), "false")]
		[InlineData(true, typeof(int), 1)]
		[InlineData(false, typeof(int), 0)]
		[InlineData(true, typeof(short), 1)]
		[InlineData(false, typeof(short), 0)]
		[InlineData(true, typeof(long), 1L)]
		[InlineData(false, typeof(long), 0L)]
		[InlineData(true, typeof(float), 1f)]
		[InlineData(false, typeof(float), 0f)]
		[InlineData(true, typeof(double), 1d)]
		[InlineData(false, typeof(double), 0d)]
		[InlineData(true, typeof(uint), (uint)1)]
		[InlineData(false, typeof(uint), (uint)0)]
		[InlineData(true, typeof(ushort), (ushort)1)]
		[InlineData(false, typeof(ushort), (ushort)0)]
		[InlineData(true, typeof(ulong), (ulong)1)]
		[InlineData(false, typeof(ulong), (ulong)0)]
		[InlineData(true, typeof(byte), (byte)1)]
		[InlineData(false, typeof(byte), (byte)0)]
		[InlineData(true, typeof(sbyte), (sbyte)1)]
		[InlineData(false, typeof(sbyte), (sbyte)0)]
		public void ConvertValid(bool value, Type destTpe, object expected) {
			var b = (SqlBoolean) value;
			var result = Convert.ChangeType(b, destTpe, CultureInfo.InvariantCulture);

			Assert.NotNull(result);
			Assert.IsType(destTpe, result);
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(true, typeof(DateTime))]
		[InlineData(false, typeof(DateTime))]
		[InlineData(true, typeof(char))]
		[InlineData(false, typeof(char))]
		public void ConvertInvalid(bool value, Type destType) {
			var b = (SqlBoolean) value;
			Assert.Throws<InvalidCastException>(() => Convert.ChangeType(b, destType, CultureInfo.InvariantCulture));
		}

		[Theory]
		[InlineData(true, 1)]
		[InlineData(false, 0)]
		public static void ConvertToSqlNumber(bool value, int expected) {
			var number = (SqlNumber) expected;
			var b = (SqlBoolean) value;

			var result = Convert.ChangeType(b, typeof(SqlNumber));
			Assert.Equal(number, result);
		}

		[Theory]
		[InlineData(true, true, false)]
		[InlineData(true, false, true)]
		[InlineData(false, false, false)]
		public void XOr(bool b1, bool b2, bool expected) {
			var value1 = (SqlBoolean)b1;
			var value2 = (SqlBoolean)b2;

			var result = value1 ^ value2;

			var bResult = (bool)result;

			Assert.Equal(expected, bResult);
		}

		[Theory]
		[InlineData(true, true, true)]
		[InlineData(true, false, true)]
		[InlineData(false, false, false)]
		public void Or(bool b1, bool b2, bool expected) {
			var value1 = (SqlBoolean)b1;
			var value2 = (SqlBoolean)b2;

			var result = value1 | value2;

			var bResult = (bool)result;

			Assert.Equal(expected, bResult);
		}

		[Theory]
		[InlineData("true", true)]
		[InlineData("TRUE", true)]
		[InlineData("TrUe", true)]
		[InlineData("FALSE", false)]
		[InlineData("false", false)]
		[InlineData("FaLsE", false)]
		[InlineData("1", true)]
		[InlineData("0", false)]
		public void Parse(string s, bool expected) {
			var result = SqlBoolean.Parse(s);

			Assert.Equal((SqlBoolean) expected, result);
		}

		[Theory]
		[InlineData("true", true, true)]
		[InlineData("TRUE", true, true)]
		[InlineData("TrUe", true, true)]
		[InlineData("FALSE", false, true)]
		[InlineData("false", false, true)]
		[InlineData("FaLsE", false, true)]
		[InlineData("1", true, true)]
		[InlineData("0", false, true)]
		[InlineData("", false, false)]
		[InlineData("445", false, false)]
		[InlineData("t rue", false, false)]
		public void TryParse(string s, bool expected, bool success) {
			SqlBoolean value;
			Assert.Equal(success, SqlBoolean.TryParse(s, out value));
			Assert.Equal(expected, (bool) value);
		}
	}
}