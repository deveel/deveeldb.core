using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlExpressionTests {
		[Theory]
		[InlineData("a.b", true)]
		public static void CreateReferenceAssign(string name, object value) {
			var objName = ObjectName.Parse(name);
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var refAssign = SqlExpression.ReferenceAssign(objName, exp);

			Assert.NotNull(refAssign.ReferenceName);
			Assert.NotNull(refAssign.Value);

			Assert.Equal(objName, refAssign.ReferenceName);
		}

		[Theory]
		[InlineData("a.b", true)]
		public static void SerializeReferenceAssign(string name, object value) {
			var objName = ObjectName.Parse(name);
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var refAssign = SqlExpression.ReferenceAssign(objName, exp);
			var result = BinarySerializeUtil.Serialize(refAssign);

			Assert.Equal(objName, result.ReferenceName);
			Assert.IsType<SqlConstantExpression>(result.Value);
		}


		[Theory]
		[InlineData("a.b", true, "a.b = TRUE")]
		public static void GetReferenceAssignString(string name, object value, string expected) {
			var objName = ObjectName.Parse(name);
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var refAssign = SqlExpression.ReferenceAssign(objName, exp);
			var sql = refAssign.ToString();

			Assert.Equal(expected, sql);
		}

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
		public static async Task  ReduceCondition(bool test, object ifTrue, object ifFalse, object expected) {
			var testExp = SqlExpression.Constant(SqlObject.Boolean(test));
			var ifTrueExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifTrue)));
			var ifFalseExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifFalse)));

			var condition = SqlExpression.Condition(testExp, ifTrueExp, ifFalseExp);

			var result = await condition.ReduceAsync(null);

			Assert.IsType<SqlConstantExpression>(result);

			var expectedValue = SqlObject.New(SqlValueUtil.FromObject(expected));
			Assert.Equal(expectedValue, ((SqlConstantExpression)result).Value);
		}

		[Fact]
		public static async Task ReduceGroup() {
			var exp = SqlExpression.Binary(SqlExpressionType.Equal,
				SqlExpression.Constant(SqlObject.Boolean(true)),
				SqlExpression.Constant(SqlObject.Boolean(false)));
			var group = SqlExpression.Group(exp);

			var reduced = await group.ReduceAsync(null);
			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);
			Assert.IsType<SqlObject>(((SqlConstantExpression)reduced).Value);
		}

		[Fact]
		public static void GetGroupString() {
			var exp = SqlExpression.Binary(SqlExpressionType.Equal,
				SqlExpression.Constant(SqlObject.Integer(33)),
				SqlExpression.Constant(SqlObject.Integer(54)));
			var group = SqlExpression.Group(exp);

			const string expected = "(33 = 54)";
			var sql = group.ToString();

			Assert.Equal(expected, sql);
		}

		[Fact]
		public static void SerializeGroup() {
			var exp = SqlExpression.Binary(SqlExpressionType.Equal,
				SqlExpression.Constant(SqlObject.Integer(33)),
				SqlExpression.Constant(SqlObject.Integer(54)));
			var group = SqlExpression.Group(exp);

			var result = BinarySerializeUtil.Serialize(group);

			Assert.NotNull(result);
			Assert.IsType<SqlBinaryExpression>(result.Expression);
		}
	}
}