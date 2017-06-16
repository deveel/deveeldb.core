using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlUnaryExpressionTests {
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
		[InlineData(SqlExpressionType.UnaryPlus, 22.34)]
		[InlineData(SqlExpressionType.Negate, 455.43)]
		[InlineData(SqlExpressionType.Not, true)]
		public static void SerializeUnary(SqlExpressionType expressionType, object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			var result = BinarySerializeUtil.Serialize(exp);

			Assert.Equal(exp.ExpressionType, result.ExpressionType);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34, 22.34)]
		[InlineData(SqlExpressionType.Negate, 455.43, -455.43)]
		[InlineData(SqlExpressionType.Not, true, false)]
		public static async Task ReduceUnary(SqlExpressionType expressionType, object value, object expected) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			Assert.NotNull(exp.Operand);
			Assert.IsType<SqlConstantExpression>(exp.Operand);

			Assert.True(exp.CanReduce);

			var reduced = await exp.ReduceAsync(null);

			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);

			var expectedResult = SqlObject.New(SqlValueUtil.FromObject(expected));
			Assert.Equal(expectedResult, ((SqlConstantExpression)reduced).Value);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34, "+22.34")]
		[InlineData(SqlExpressionType.Negate, 455.43, "-455.43")]
		[InlineData(SqlExpressionType.Not, true, "~TRUE")]
		public static void GetUnaryString(SqlExpressionType expressionType, object value, string expected) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			var s = exp.ToString();
			Assert.Equal(expected, s);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34)]
		[InlineData(SqlExpressionType.Negate, 455.43)]
		[InlineData(SqlExpressionType.Not, true)]
		public static void GetUnaryType(SqlExpressionType expressionType, object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			var sqlType = exp.Type;
			Assert.Equal(obj.Type, sqlType);
		}

		[Theory]
		[InlineData("+454.90", SqlExpressionType.UnaryPlus)]
		[InlineData("NOT TRUE", SqlExpressionType.Not)]
		[InlineData("-7849", SqlExpressionType.Negate)]
		public static void ParseString(string s, SqlExpressionType expressionType) {
			var exp = SqlExpression.Parse(s);

			Assert.NotNull(exp);
			Assert.Equal(expressionType, exp.ExpressionType);
			Assert.IsType<SqlUnaryExpression>(exp);
		}
	}
}