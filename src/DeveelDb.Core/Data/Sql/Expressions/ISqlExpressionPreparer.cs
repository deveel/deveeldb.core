using System;

namespace Deveel.Data.Sql.Expressions {
	public interface ISqlExpressionPreparer {
		bool CanPrepare(SqlExpression expression);

		SqlExpression Prepare(SqlExpression expression);
	}
}