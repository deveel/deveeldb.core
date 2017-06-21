using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlConditionExpressionTests {
		[Theory]
		[InlineData(true, 223.21, 11, "CASE WHEN TRUE THEN 223.21 ELSE 11 END")]
		public static void GetConditionString(bool test, object ifTrue, object ifFalse, string expected) {
			var testExp = SqlExpression.Constant(SqlObject.Boolean(test));
			var ifTrueExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifTrue)));
			var ifFalseExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifFalse)));

			var condition = SqlExpression.Condition(testExp, ifTrueExp, ifFalseExp);

			var sql = condition.ToString();
			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("CASE a WHEN 1 THEN TRUE ELSE FALSE END")]
		[InlineData("CASE WHEN a = 1 THEN TRUE ELSE FALSE END")]
		[InlineData("CASE a WHEN 1 THEN TRUE WHEN 2 THEN TRUE ELSE FALSE END")]
		[InlineData("CASE WHEN a = 1 THEN TRUE WHEN b = 2 THEN FALSE END")]
		public static void ParseSipleCaseString(string s) {
			var exp = SqlExpression.Parse(s);

			Assert.NotNull(exp);
			Assert.IsType<SqlConditionExpression>(exp);

			var condition = (SqlConditionExpression) exp;

			Assert.NotNull(condition.Test);
			Assert.NotNull(condition.IfTrue);
			Assert.NotNull(condition.IfFalse);
		}

		[Theory]
		[InlineData(true, 223.21, 11)]
		public static void SerializeCondition(bool test, object ifTrue, object ifFalse) {
			var testExp = SqlExpression.Constant(SqlObject.Boolean(test));
			var ifTrueExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifTrue)));
			var ifFalseExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifFalse)));

			var condition = SqlExpression.Condition(testExp, ifTrueExp, ifFalseExp);
			var result = BinarySerializeUtil.Serialize(condition);

			Assert.NotNull(result.Test);
			Assert.NotNull(result.IfTrue);
			Assert.NotNull(result.IfFalse);
		}


		[Theory]
		[InlineData(true, "I am", "You are", "I am")]
		[InlineData(false, "I am", "You are", "You are")]
		public static async Task ReduceCondition(bool test, object ifTrue, object ifFalse, object expected) {
			var testExp = SqlExpression.Constant(SqlObject.Boolean(test));
			var ifTrueExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifTrue)));
			var ifFalseExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifFalse)));

			var condition = SqlExpression.Condition(testExp, ifTrueExp, ifFalseExp);

			var result = await condition.ReduceAsync(null);

			Assert.IsType<SqlConstantExpression>(result);

			var expectedValue = SqlObject.New(SqlValueUtil.FromObject(expected));
			Assert.Equal(expectedValue, ((SqlConstantExpression)result).Value);
		}
	}
}