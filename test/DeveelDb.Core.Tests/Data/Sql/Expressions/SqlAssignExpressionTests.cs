using System;

using Deveel.Data.Serialization;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlAssignExpressionTests {
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
	}
}