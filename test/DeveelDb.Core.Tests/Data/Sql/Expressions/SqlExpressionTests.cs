using System;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlExpressionTests {
		[Theory]
		[InlineData(65775.499)]
		[InlineData("The quick brown fox")]
		public static void CreateConstant(object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			Assert.NotNull(exp.Value);
			Assert.Equal(obj, exp.Value);
		}


		[Theory]
		[InlineData(SqlExpressionType.Equal, 6577.494, 449.004)]
		[InlineData(SqlExpressionType.Add, 323, 12)]
		public static void CreateBinary(SqlExpressionType expressionType, object value1, object value2) {
			var obj1 = SqlObject.New(SqlValueUtil.FromObject(value1));
			var obj2 = SqlObject.New(SqlValueUtil.FromObject(value2));

			var exp = SqlExpression.Binary(expressionType, SqlExpression.Constant(obj1), SqlExpression.Constant(obj2));

			Assert.NotNull(exp);
			Assert.NotNull(exp.Left);
			Assert.NotNull(exp.Right);
		}

		[Theory]
		[InlineData(345)]
		public static void ReduceConstant(object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			Assert.False(exp.CanReduce);
			Assert.NotNull(exp.Value);

			var reduced = exp.Reduce(null);

			Assert.IsType<SqlConstantExpression>(reduced);
			Assert.Equal(obj, ((SqlConstantExpression) reduced).Value);
		}

		[Theory]
		[InlineData(SqlExpressionType.Equal, 34, 34, true)]
		[InlineData(SqlExpressionType.NotEqual, 190, 21, true)]
		[InlineData(SqlExpressionType.GreaterThan, 12.02e32, 23.98, true)]
		[InlineData(SqlExpressionType.GreaterThanOrEqual, 110, 20, true)]
		[InlineData(SqlExpressionType.LessThan, 67, 98, true)]
		[InlineData(SqlExpressionType.LessThanOrEqual, "abc1234", "abc12345", false)]
		[InlineData(SqlExpressionType.Add, 45, 45, 90)]
		[InlineData(SqlExpressionType.Subtract, 102, 30, 72)]
		[InlineData(SqlExpressionType.Multiply, 22, 2, 44)]
		[InlineData(SqlExpressionType.Divide, 100, 2, 50)]
		[InlineData(SqlExpressionType.Is, true, true, true)]
		[InlineData(SqlExpressionType.IsNot, 22.09, false, true)]
		[InlineData(SqlExpressionType.Or, true, false, true)]
		[InlineData(SqlExpressionType.XOr, 113, 56, 73)]
		[InlineData(SqlExpressionType.And, true, false, false)]
		public static void ReduceBinary(SqlExpressionType expressionType, object value1, object value2, object expected) {
			var obj1 = SqlObject.New(SqlValueUtil.FromObject(value1));
			var obj2 = SqlObject.New(SqlValueUtil.FromObject(value2));

			var exp = SqlExpression.Binary(expressionType, SqlExpression.Constant(obj1), SqlExpression.Constant(obj2));

			Assert.True(exp.CanReduce);

			var reduced = exp.Reduce(null);

			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);

			var result = ((SqlConstantExpression) reduced).Value;
			var expectedResult = SqlObject.New(SqlValueUtil.FromObject(expected));

			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData(SqlExpressionType.Equal, 34, 34, "34 = 34")]
		[InlineData(SqlExpressionType.NotEqual, 190, 21, "190 <> 21")]
		[InlineData(SqlExpressionType.GreaterThan, 12.02, 23.98, "12.02000000000000 > 23.98000000000000")]
		[InlineData(SqlExpressionType.GreaterThanOrEqual, 110, 20, "110 >= 20")]
		[InlineData(SqlExpressionType.LessThan, 67, 98, "67 < 98")]
		[InlineData(SqlExpressionType.LessThanOrEqual, "abc1234", "abc12345", "'abc1234' <= 'abc12345'")]
		[InlineData(SqlExpressionType.Add, 45, 45, "45 + 45")]
		[InlineData(SqlExpressionType.Subtract, 102, 30, "102 - 30")]
		[InlineData(SqlExpressionType.Multiply, 22, 2, "22 * 2")]
		[InlineData(SqlExpressionType.Divide, 100, 2, "100 / 2")]
		[InlineData(SqlExpressionType.Is, true, true, "TRUE IS TRUE")]
		[InlineData(SqlExpressionType.IsNot, 22.09, false, "22.09000000000000 IS NOT FALSE")]
		[InlineData(SqlExpressionType.Or, true, false, "TRUE OR FALSE")]
		[InlineData(SqlExpressionType.XOr, 113, 56, "113 XOR 56")]
		[InlineData(SqlExpressionType.And, true, false, "TRUE AND FALSE")]
		public static void GetBinaryString(SqlExpressionType expressionType, object value1, object value2, string expected) {
			var obj1 = SqlObject.New(SqlValueUtil.FromObject(value1));
			var obj2 = SqlObject.New(SqlValueUtil.FromObject(value2));

			var exp = SqlExpression.Binary(expressionType, SqlExpression.Constant(obj1), SqlExpression.Constant(obj2));

			var s = exp.ToString();
			Assert.Equal(expected, s);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34)]
		[InlineData(SqlExpressionType.Negate, 455.43)]
		[InlineData(SqlExpressionType.Not, true)]
		public static void CreateUnary(SqlExpressionType expressionType, object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			Assert.NotNull(exp.Operand);
			Assert.IsType<SqlConstantExpression>(exp.Operand);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34, 22.34)]
		[InlineData(SqlExpressionType.Negate, 455.43, -455.43)]
		[InlineData(SqlExpressionType.Not, true, false)]
		public static void ReduceUnary(SqlExpressionType expressionType, object value, object expected) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			Assert.NotNull(exp.Operand);
			Assert.IsType<SqlConstantExpression>(exp.Operand);

			Assert.True(exp.CanReduce);

			var reduced = exp.Reduce(null);

			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);

			var expectedResult = SqlObject.New(SqlValueUtil.FromObject(expected));
			Assert.Equal(expectedResult, ((SqlConstantExpression) reduced).Value);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34, "+22.34000000000000")]
		[InlineData(SqlExpressionType.Negate, 455.43, "-455.4300000000000")]
		[InlineData(SqlExpressionType.Not, true, "~TRUE")]
		public static void GetUnaryString(SqlExpressionType expressionType, object value, string expected) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			var s = exp.ToString();
			Assert.Equal(expected, s);
		}

		[Theory]
		[InlineData(5634.99, SqlTypeCode.Double, -1, -1)]
		public static void NewCast(object value, SqlTypeCode destTypeCode, int p, int s) {
			var targetType = PrimitiveTypes.Type(destTypeCode, new {precision = p, scale = s, maxSize = p, size = p});
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var cast = SqlExpression.Cast(exp, targetType);

			Assert.NotNull(cast.Value);
			Assert.NotNull(cast.TargetType);
		}

		[Theory]
		[InlineData(5634.99, SqlTypeCode.Double, -1, -1, (double) 5634.99)]
		[InlineData("true", SqlTypeCode.Boolean, -1, -1, true)]
		public static void ReduceCast(object value, SqlTypeCode destTypeCode, int p, int s, object expected) {
			var targetType = PrimitiveTypes.Type(destTypeCode, new {precision = p, scale = s, maxSize = p, size = p});
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var cast = SqlExpression.Cast(exp, targetType);

			Assert.True(cast.CanReduce);

			var reduced = cast.Reduce(null);
			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);

			var result = ((SqlConstantExpression) reduced).Value;

			Assert.NotNull(result);
			Assert.Equal(destTypeCode, result.Type.TypeCode);

			var expectedResult = SqlObject.New(SqlValueUtil.FromObject(expected));
			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData(5634.99, SqlTypeCode.Double, -1, -1, "CAST(5634.990000000000 AS DOUBLE)")]
		[InlineData("TRUE", SqlTypeCode.Boolean, -1, -1, "CAST('TRUE' AS BOOLEAN)")]
		public static void GetCastString(object value, SqlTypeCode destTypeCode, int p, int s, string expected) {
			var targetType = PrimitiveTypes.Type(destTypeCode, new {precision = p, scale = s, maxSize = p, size = p});
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var cast = SqlExpression.Cast(exp, targetType);
			var sql = cast.ToString();

			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("a.*")]
		[InlineData("a.b.c")]
		public static void CreateReference(string name) {
			var objName = ObjectName.Parse(name);
			var exp = SqlExpression.Reference(objName);

			Assert.NotNull(exp.ReferenceName);
			Assert.Equal(objName, exp.ReferenceName);
		}

		[Theory]
		[InlineData("a.*")]
		[InlineData("a.b.c")]
		public static void GetReferenceString(string name) {
			var objName = ObjectName.Parse(name);
			var exp = SqlExpression.Reference(objName);

			var sql = exp.ToString();
			Assert.Equal(name, sql);
		}
	}
}