using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlGroupExpressionTests {
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