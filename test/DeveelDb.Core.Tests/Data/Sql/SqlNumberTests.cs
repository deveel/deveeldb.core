using System;
using System.ComponentModel;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlNumberTests {
		[Fact]
		public static void Create_FromInteger() {
			var value = new SqlNumber((int)45993);
			Assert.False(value.IsNull);
			Assert.True(value.CanBeInt32);
			Assert.True(value.CanBeInt64);
			Assert.Equal(0, value.Scale);
			Assert.Equal(5, value.Precision);
			Assert.False(SqlNumber.IsNan(value));
			Assert.False(SqlNumber.IsNegativeInfinity(value));
			Assert.False(SqlNumber.IsPositiveInfinity(value));
			Assert.Equal(1, value.Sign);
		}

		[Fact]
		public static void Create_FromBigInt() {
			var value = new SqlNumber(4599356655L);
			Assert.False(value.IsNull);
			Assert.False(value.CanBeInt32);
			Assert.True(value.CanBeInt64);
			Assert.Equal(0, value.Scale);
			Assert.Equal(10, value.Precision);
			Assert.False(SqlNumber.IsNan(value));
			Assert.False(SqlNumber.IsNegativeInfinity(value));
			Assert.False(SqlNumber.IsPositiveInfinity(value));
			Assert.Equal(1, value.Sign);
		}

		[Fact]
		public static void Create_FromDouble() {
			var value = new SqlNumber(459935.9803d);
			Assert.False(value.IsNull);
			Assert.False(value.CanBeInt32);
			Assert.False(value.CanBeInt64);
			Assert.Equal(10, value.Scale);
			Assert.Equal(16, value.Precision);
			Assert.False(SqlNumber.IsNan(value));
			Assert.False(SqlNumber.IsNegativeInfinity(value));
			Assert.False(SqlNumber.IsPositiveInfinity(value));
			Assert.Equal(1, value.Sign);
		}

		[Fact]
		public static void Parse_BigDecimal() {
			var value = SqlNumber.Parse("98356278.911288837773848500069994933229238e45789");
			Assert.False(value.IsNull);
			Assert.False(value.CanBeInt32);
			Assert.False(value.CanBeInt64);
			Assert.True(value.Precision > 40);
		}

		[Fact]
		public static void ParseInfinity() {
			SqlNumber number;
			Assert.True(SqlNumber.TryParse("+Infinity", out number));
			Assert.NotNull(number);
			Assert.False(number.IsNull);
			Assert.Equal(SqlNumber.PositiveInfinity, number);

			Assert.True(SqlNumber.TryParse("-Infinity", out number));
			Assert.NotNull(number);
			Assert.False(number.IsNull);
			Assert.Equal(SqlNumber.NegativeInfinity, number);
		}

		[Fact]
		public static void ParseNaN() {
			SqlNumber number;
			Assert.True(SqlNumber.TryParse("NaN", out number));
			Assert.NotNull(number);
			Assert.False(number.IsNull);
			Assert.Equal(SqlNumber.NaN, number);
		}

		[Fact]
		public static void Convert_ToBoolean_Success() {
			var value = SqlNumber.One;
			var b = (SqlBoolean)Convert.ChangeType(value, typeof(SqlBoolean));
			Assert.True(b);
		}

		[Fact]
		public static void Integer_Greater_True() {
			var value1 = new SqlNumber(76);
			var value2 = new SqlNumber(54);

			Assert.True(value1 > value2);
		}

		[Fact]
		public static void BigNumber_Greater_True() {
			var value1 = SqlNumber.Parse("98356278.911288837773848500069994933229238e45789");
			var value2 = SqlNumber.Parse("348299.01991828833333333333488888388829911182227373738488847112349928");

			Assert.True(value1 > value2);
		}

		[InlineData(34)]
		[InlineData(12784774)]
		public static void Int32_Convert(int value) {
			var number = new SqlNumber(value);

			var result = Convert.ChangeType(number, typeof(int));

			Assert.IsType<int>(result);
			Assert.Equal(value, (int)result);
		}

		[Theory]
		[InlineData(9010)]
		[InlineData(87749948399)]
		public static void Int64_Convert(long value) {
			var number = new SqlNumber(value);

			var result = Convert.ChangeType(number, typeof(long));

			Assert.IsType<long>(result);
			Assert.Equal(value, (long)result);
		}

		[Theory]
		[InlineData(90.121)]
		[InlineData(119299.0029)]
		public static void Double_Convert(double value) {
			var number = new SqlNumber(value);

			var result = Convert.ChangeType(number, typeof(double));

			Assert.IsType<double>(result);
			Assert.Equal(value, (double)result);
		}

		[Theory]
		[InlineData(100)]
		[InlineData(2)]
		public static void Byte_Convert(byte value) {
			var number = new SqlNumber(value);

			var result = Convert.ChangeType(number, typeof(byte));

			Assert.IsType<byte>(result);
			Assert.Equal(value, (byte)result);
		}

		[Theory]
		[InlineData(466637, 9993, 476630)]
		public static void Operator_Add(int value1, int value2, int expected) {
			var num1 = new SqlNumber(value1);
			var num2 = new SqlNumber(value2);

			var result = num1 + num2;

			Assert.NotNull(result);
			Assert.False(result.IsNull);
			Assert.True(result.CanBeInt32);

			var intResult = result.ToInt32();

			Assert.Equal(expected, intResult);
		}

		[Theory]
		[InlineData(5455261, 119020, 5336241)]
		public static void Operator_Subtract(int value1, int value2, int expected) {
			var num1 = new SqlNumber(value1);
			var num2 = new SqlNumber(value2);

			var result = num1 - num2;

			Assert.NotNull(result);
			Assert.False(result.IsNull);
			Assert.True(result.CanBeInt32);

			var intResult = result.ToInt32();

			Assert.Equal(expected, intResult);
		}

		[Theory]
		[InlineData(2783, 231, 642873)]
		public static void Operator_Multiply(int value1, int value2, int expected) {
			var num1 = new SqlNumber(value1);
			var num2 = new SqlNumber(value2);

			var result = num1 * num2;

			Assert.NotNull(result);
			Assert.False(result.IsNull);
			Assert.True(result.CanBeInt32);

			var intResult = result.ToInt32();

			Assert.Equal(expected, intResult);
		}

		[Theory]
		[InlineData(4533, 90, 33)]
		public static void Operator_Modulo(int value1, int value2, float expected) {
			var number1 = new SqlNumber(value1);
			var number2 = new SqlNumber(value2);

			var result = number1 % number2;

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(1152663, 9929, 116.0905428543)]
		[InlineData(40, 5, 8)]
		public static void Operator_Divide(int value1, int value2, double expected) {
			var num1 = new SqlNumber(value1);
			var num2 = new SqlNumber(value2);

			var result = num1 / num2;

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(7782, -7782)]
		[InlineData(-9021, 9021)]
		public static void Operator_Negate(int value, int expected) {
			var number = new SqlNumber(value);

			var result = -number;

			Assert.NotNull(result);
			Assert.False(result.IsNull);
			Assert.True(result.CanBeInt32);

			Assert.Equal(expected, result.ToInt32());
		}

		[Theory]
		[InlineData(7782, 7782)]
		[InlineData(-9021, -9021)]
		public static void Operator_Plus(int value, int expected) {
			var number = new SqlNumber(value);

			var result = +number;

			Assert.NotNull(result);
			Assert.False(result.IsNull);
			Assert.True(result.CanBeInt32);

			Assert.Equal(expected, result.ToInt32());
		}

		[Theory]
		[InlineData(46677, 9982, false)]
		[InlineData(92677, 92677, true)]
		public static void Operator_Int32Equal(int value1, int value2, bool expected) {
			var num1 = new SqlNumber(value1);
			var num2 = new SqlNumber(value2);

			var result = num1 == num2;

			Assert.Equal(expected, result);
		}

		[Fact]
		public static void Operator_EqualToNull() {
			var number = new SqlNumber(563663.9920);

			var result = number == null;

			Assert.Equal(false, result);
		}

		[Theory]
		[InlineData(46677, 9982, true)]
		[InlineData(92677, 92677, false)]
		public static void Operator_Int32NotEqual(int value1, int value2, bool expected) {
			var num1 = new SqlNumber(value1);
			var num2 = new SqlNumber(value2);

			var result = num1 != num2;

			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData(455, 3, 94196375)]
		public static void Function_Pow(int value, int exp, double expected) {
			var number = new SqlNumber(value);
			var result = number.Pow(new SqlNumber(exp));

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(99820, 48993, 1.0659007887179619)]
		public static void Function_Log(int value, int newBase, double expected) {
			var number = new SqlNumber(value);
			var result = number.Log(new SqlNumber(newBase));

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(9963, -0.53211858514845722)]
		public static void Function_Cos(int value, double expected) {
			var number = new SqlNumber(value);
			var result = number.Cos();

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(0.36f, 1.0655028755774869)]
		public static void Function_CosH(float value, double expected) {
			var number = new SqlNumber(value);
			var result = number.CosH();

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(-45636.0003922, 45636.0003922)]
		public static void Function_Abs(double value, double expected) {
			var number = new SqlNumber(value);
			var result = number.Abs();

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(559604.003100, 23.625265230100389)]
		public static void Function_Tan(double value, double expected) {
			var number = new SqlNumber(value);
			var result = number.Tan();

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(89366647.992, 1)]
		public static void Function_TanH(double value, double expected) {
			var number = new SqlNumber(value);
			var result = number.TanH();

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(929928.00111992934, 929928.00111992937)]
		public static void Function_Round(double value, double expected) {
			var number = new SqlNumber(value);
			var result = number.Round();

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(929928.00111992934, 10, 929928.0011)]
		public static void Function_RoundWithPrecision(double value, int precision, double expected) {
			var number = new SqlNumber(value);
			var result = number.Round(precision);

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}

		[Theory]
		[InlineData(02993011.338, -0.3040696985546506)]
		public static void Function_Sin(double value, double expected) {
			var number = new SqlNumber(value);
			var result = number.Sin();

			Assert.NotNull(result);
			Assert.False(result.IsNull);

			var doubleResult = result.ToDouble();

			Assert.Equal(expected, doubleResult);
		}
	}
}