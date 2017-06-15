using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public class SqlConstantExpressionTests {
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
		[InlineData(65775.499)]
		[InlineData("The quick brown fox")]
		public static void SerializeConstant(object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var result = BinarySerializeUtil.Serialize(exp);

			Assert.Equal(exp.ExpressionType, result.ExpressionType);
			Assert.Equal(exp.Value, result.Value);
		}

		[Theory]
		[InlineData(345)]
		public static async void ReduceConstant(object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			Assert.False(exp.CanReduce);
			Assert.NotNull(exp.Value);

			var reduced = await exp.ReduceAsync(null);

			Assert.IsType<SqlConstantExpression>(reduced);
			Assert.Equal(obj, ((SqlConstantExpression)reduced).Value);
		}
	}
}
