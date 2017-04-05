using System;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlNumericTypeTests {
		[Theory]
		[InlineData(SqlTypeCode.Integer, 10, 0)]
		[InlineData(SqlTypeCode.Numeric, 20, 15)]
		[InlineData(SqlTypeCode.BigInt, 19, 0)]
		[InlineData(SqlTypeCode.Numeric, 21, 10)]
		public static void CreateNumericType(SqlTypeCode typeCode, int precision, int scale) {
			var type = new SqlNumericType(typeCode, precision, scale);

			Assert.NotNull(type);
			Assert.Equal(typeCode, type.TypeCode);
			Assert.Equal(precision, type.Precision);
			Assert.Equal(scale, type.Scale);
			Assert.True(type.IsIndexable);
			Assert.True(type.IsPrimitive);
			Assert.False(type.IsLargeObject);
			Assert.False(type.IsReference);
		}

		[Theory]
		[InlineData(4553.0944, 4553.0944, true)]
		[InlineData(322, 321, false)]
		public static void NumbersEqual(double value1, double value2, bool expected) {
			BinaryOp(type => type.Equal, value1, value2, expected);
		}

		[Theory]
		[InlineData(10020, 21002, false)]
		[InlineData(32.1002, 31.223334, true)]
		[InlineData(10223933, 1233.903, true)]
		public static void NumberGreater(double value1, double value2, bool expected) {
			BinaryOp(type => type.Greater, value1, value2, expected);
		}

		[Theory]
		[InlineData(3212, 1022333.322, true)]
		[InlineData(2123e89, 102223e21, false)]
		[InlineData(122, 100, false)]
		public static void NumberSmaller(double value1, double value2, bool expected) {
			BinaryOp(type => type.Less, value1, value2, expected);
		}

		[Theory]
		[InlineData(2344, 23456, false)]
		[InlineData(1233, 1233, true)]
		[InlineData(4321.34e32, 2112.21e2, true)]
		public static void NumberGreateOrEqual(double value1, double value2, bool expected) {
			BinaryOp(type => type.GreaterOrEqual, value1, value2, expected);
		}


		[Theory]
		[InlineData(2133, 100, false)]
		[InlineData(210, 4355e45, true)]
		public static void NumberSmallerOrEqual(double value1, double value2, bool expected) {
			BinaryOp(type => type.LessOrEqual, value1, value2, expected);
		}

		private static void BinaryOp(Func<SqlNumericType, Func<ISqlValue, ISqlValue, SqlBoolean>> selector, double value1, double value2, bool expected) {
			var number1 = (SqlNumber)value1;
			var number2 = (SqlNumber)value2;
			var type = new SqlNumericType(SqlTypeCode.Double, -1, -1);

			var op = selector(type);
			var result = op(number1, number2);

			Assert.NotNull(result);
			Assert.Equal(expected, (bool)result);
		}
	}
}