using System;

namespace Deveel.Data.Sql.Expressions {
	public interface ISqlExpressionPreparable<TResult> {
		TResult PrepareExpressions(ISqlExpressionPreparer preparer);
	}
}