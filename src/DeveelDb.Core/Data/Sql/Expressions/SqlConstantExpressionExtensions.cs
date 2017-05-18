using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Expressions {
	static class SqlConstantExpressionExtensions {
		public static async Task<SqlObject> ReduceToConstantAsync(this SqlExpression expression, IContext context) {
			var reduced = await expression.ReduceAsync(context);
			if (reduced.ExpressionType != SqlExpressionType.Constant)
				throw new SqlExpressionException("The expression was not reduced to constant");

			return ((SqlConstantExpression) reduced).Value;
		} 

		public static bool IsConstant(this SqlExpression expression) {
			var visitor = new SqlConstantExpressionVisitor();
			visitor.Visit(expression);
			return visitor.IsConstant;
		}

		class SqlConstantExpressionVisitor : SqlExpressionVisitor {
			public bool IsConstant { get; private set; } = true;

			public override SqlExpression Visit(SqlExpression expression) {
				if (expression.IsReference)
					IsConstant = false;

				return base.Visit(expression);
			}
		}
	}
}