using System;

namespace Deveel.Data.Sql.Expressions {
	public interface ISqlExpressionPreparable {
		object PrepareExpressions(ISqlExpressionPreparer preparer);
	}
}