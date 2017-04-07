using System;

namespace Deveel.Data.Sql.Expressions {
	static class SqlExpressionExtensions {
		public static SqlType ReturnType(this SqlExpression expression, IContext context) {
			var visitor = new SqlTypeExpressionVisitor(context);
			visitor.Visit(expression);
			return visitor.Type;
		}
	}
}