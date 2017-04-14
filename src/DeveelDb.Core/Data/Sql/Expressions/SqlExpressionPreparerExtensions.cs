using System;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlExpressionPreparerExtensions {
		public static SqlExpression Prepare(this SqlExpression expression, ISqlExpressionPreparer preparer) {
			var visitor = new SqlExpressionPrepareVisitor(preparer);
			return visitor.Visit(expression);
		}
	}
}