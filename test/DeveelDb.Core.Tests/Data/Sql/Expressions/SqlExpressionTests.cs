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
			Assert.Equal(obj, ((SqlConstantExpression)reduced).Value);
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
	}
}