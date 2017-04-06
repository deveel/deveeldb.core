﻿using System;

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
		public static void Equal(double value1, double value2, bool expected) {
			BinaryOp(type => type.Equal, value1, value2, expected);
		}

		[Theory]
		[InlineData(10020, 21002, false)]
		[InlineData(32.1002, 31.223334, true)]
		[InlineData(10223933, 1233.903, true)]
		public static void Greater(double value1, double value2, bool expected) {
			BinaryOp(type => type.Greater, value1, value2, expected);
		}

		[Theory]
		[InlineData(3212, 1022333.322, true)]
		[InlineData(2123e89, 102223e21, false)]
		[InlineData(122, 100, false)]
		public static void Smaller(double value1, double value2, bool expected) {
			BinaryOp(type => type.Less, value1, value2, expected);
		}

		[Theory]
		[InlineData(2344, 23456, false)]
		[InlineData(1233, 1233, true)]
		[InlineData(4321.34e32, 2112.21e2, true)]
		public static void GreateOrEqual(double value1, double value2, bool expected) {
			BinaryOp(type => type.GreaterOrEqual, value1, value2, expected);
		}


		[Theory]
		[InlineData(2133, 100, false)]
		[InlineData(210, 4355e45, true)]
		public static void SmallerOrEqual(double value1, double value2, bool expected) {
			BinaryOp(type => type.LessOrEqual, value1, value2, expected);
		}

		[Theory]
		[InlineData(566, -567)]
		[InlineData(789929.245, -789930)]
		[InlineData((byte)1, -2)]
		public static void Not(object value, object expected) {
			OperatorsUtil.Unary(type => type.Not, value, expected);
		}

		[Theory]
		[InlineData(112, 112)]
		[InlineData(-98.09f, -98.09f)]
		public static void UnaryPlus(object value, object expected) {
			OperatorsUtil.Unary(type => type.UnaryPlus, value, expected);
		}

		[Theory]
		[InlineData(-2536.9039, 2536.9039)]
		[InlineData(788, -788)]
		public static void Negate(object value, object expected) {
			OperatorsUtil.Unary(type => type.Negate, value, expected);
		}

		[Theory]
		[InlineData(4355, SqlTypeCode.Double, -1, -1, (double) 4355)]
		[InlineData(673.04492, SqlTypeCode.String, 200, -1, "673.0449200000000")]
		[InlineData(6709.89f, SqlTypeCode.Char, 7, -1, "6709.89")]
		[InlineData((byte)23, SqlTypeCode.Double, -1, -1, (double)23)]
		[InlineData(32167, SqlTypeCode.Float, -1, -1, (float)32167)]
		[InlineData((double)56878.99876, SqlTypeCode.Float, -1, -1, (float) 56878.99876)]
		private static void Cast(object value, SqlTypeCode destType, int p, int s, object expected) {
			OperatorsUtil.Cast(value, destType, p, s, expected);
		}

		private static void BinaryOp(Func<SqlType, Func<ISqlValue, ISqlValue, SqlBoolean>> selector, object value1, object value2, bool expected) {
			OperatorsUtil.Binary(PrimitiveTypes.Double(), selector, value1, value2, expected);
		}

		private static void Unary(Func<SqlType, Func<ISqlValue, ISqlValue>> selector, object value, object result) {
			OperatorsUtil.Unary(PrimitiveTypes.Double(), selector, value, result);
		}
	}
}