using System;
using System.ComponentModel;

using Xunit;

namespace Deveel.Data.Sql {
	public class SqlBooleanTest {
		[Fact]
		public void CreateFromByte() {
			var value = new SqlBoolean(1);
			Assert.NotNull(value);
			Assert.False(value.IsNull);
			Assert.Equal(true, (bool)value);

			value = new SqlBoolean(0);
			Assert.NotNull(value);
			Assert.False(value.IsNull);
			Assert.Equal(false, (bool)value);
		}

		[Fact]
		public void CreateFromBoolean() {
			var value = new SqlBoolean(true);
			Assert.NotNull(value);
			Assert.False(value.IsNull);
			Assert.Equal(true, (bool)value);

			value = new SqlBoolean(false);
			Assert.NotNull(value);
			Assert.False(value.IsNull);
			Assert.Equal(false, (bool)value);
		}

		[Fact]
		public void Compare_Equal() {
			var value1 = SqlBoolean.True;
			var value2 = new SqlBoolean(true);

			Assert.False(value1.IsNull);
			Assert.False(value2.IsNull);

			Assert.True(value1.IsComparableTo(value2));

			int i = -2;
			i = value1.CompareTo(value2);
			Assert.Equal(0, i);
		}

		[Fact]
		public void Compare_NotEqual() {
			var value1 = SqlBoolean.False;
			var value2 = new SqlBoolean(true);

			Assert.False(value1.IsNull);
			Assert.False(value2.IsNull);

			Assert.True(value1.IsComparableTo(value2));

			int i = -2;
			i = value1.CompareTo(value2);
			Assert.Equal(-1, i);
		}

		[Fact]
		public void Compare_ToBooleanNull() {
			var value1 = SqlBoolean.True;
			var value2 = SqlBoolean.Null;

			Assert.False(value1.IsNull);
			Assert.True(value2.IsNull);

			Assert.True(value1.IsComparableTo(value2));

			int i = -2;
			i = value1.CompareTo(value2);
			Assert.Equal(1, i);
		}

		[Fact]
		public void Compare_ToNull() {
			var value1 = SqlBoolean.True;
			var value2 = SqlNull.Value;

			Assert.False(value1.IsNull);
			Assert.True(value2.IsNull);

			Assert.True(value1.IsComparableTo(value2));

			int i = -2;
			i = value1.CompareTo(value2);
			Assert.Equal(1, i);
		}

		/*
		TODO:
		[Fact]
		public void Compare_ToNumber_InRange() {
			var value1 = SqlBoolean.True;
			var value2 = SqlNumber.One;

			Assert.IsFalse(value1.IsNull);
			Assert.IsFalse(value2.IsNull);

			Assert.IsTrue(value1.IsComparableTo(value2));

			int i = -2;
			Assert.DoesNotThrow(() => i = value1.CompareTo(value2));
			Assert.AreEqual(0, i);

			value2 = SqlNumber.Zero;

			Assert.IsFalse(value1.IsNull);
			Assert.IsFalse(value2.IsNull);

			Assert.IsTrue(value1.IsComparableTo(value2));

			i = -2;
			Assert.DoesNotThrow(() => i = value1.CompareTo(value2));
			Assert.AreEqual(1, i);
		}

		[Fact]
		public void Compare_ToNumber_OutOfRange() {
			var value1 = SqlBoolean.True;
			var value2 = new SqlNumber(21);

			Assert.IsFalse(value1.IsNull);
			Assert.IsFalse(value2.IsNull);

			Assert.IsFalse(value1.IsComparableTo(value2));

			int i = -2;
			Assert.Throws<ArgumentOutOfRangeException>(() => i = value1.CompareTo(value2));
			Assert.AreEqual(-2, i);
		}
		*/

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

			value2 = SqlBoolean.Null;

			Assert.True(value1 != value2);
		}

		[Fact]
		public void Equality_ToNull_True() {
			var value1 = SqlBoolean.Null;
			var value2 = SqlNull.Value;

			Assert.True(value1 == value2);
		}

		[Fact]
		[Category("Conversion")]
		public void Convert_ToString() {
			var value = SqlBoolean.True;
			Assert.Equal("true", value.ToString());

			value = SqlBoolean.False;
			Assert.Equal("false", value.ToString());

			value = SqlBoolean.Null;
			Assert.Equal("NULL", value.ToString());
		}

		[Theory]
		[InlineData(true, true, false)]
		[InlineData(true, false, true)]
		[InlineData(false, false, false)]
		public void XOr(bool b1, bool b2, bool expected) {
			var value1 = (SqlBoolean)b1;
			var value2 = (SqlBoolean)b2;

			var result = value1.XOr(value2);

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
		[InlineData("NULL", null)]
		[InlineData("null", null)]
		[InlineData("1", true)]
		[InlineData("0", false)]
		public void Parse(string s, bool? expected) {
			var result = SqlBoolean.Parse(s);

			var bResult = (bool?)result;
			Assert.Equal(expected, bResult);
		}
	}
}