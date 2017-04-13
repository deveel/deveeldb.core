using System;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlConstantExpressionExtensions {
		public static bool IsConstant(this SqlExpression expression) {
			var visitor = new SqlConstantExpressionVisitor();
			visitor.Visit(expression);
			return visitor.IsConstant;
		}

		class SqlConstantExpressionVisitor : SqlExpressionVisitor {
			public bool IsConstant { get; private set; } = true;

			public override SqlExpression Visit(SqlExpression expression) {
				if (!expression.IsReference)
					IsConstant = false;

				return base.Visit(expression);
			}
		}
	}
}