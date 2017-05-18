using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Expressions {
	static class SqlExpressionExtensions {
		public static async Task<SqlObject> ReduceToConstantAsync(this SqlExpression expression, IContext context) {
			var result = await expression.ReduceAsync(context);
			if (result.ExpressionType != SqlExpressionType.Constant)
				throw new SqlExpressionException("The expression was not reduced to constant");

			return ((SqlConstantExpression) result).Value;
		}
	}
}