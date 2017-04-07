using System;

namespace Deveel.Data.Sql.Expressions {
	public interface ISqlExpressionVisitor {
		SqlExpression Visit(SqlExpression expression);
	}
}