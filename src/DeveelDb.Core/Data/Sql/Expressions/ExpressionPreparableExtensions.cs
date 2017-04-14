using System;

namespace Deveel.Data.Sql.Expressions {
	public static class ExpressionPreparableExtensions {
		public static T PrepareExpressions<T>(this T preparable, ISqlExpressionPreparer preparer) where T : ISqlExpressionPreparable<T> {
			return preparable.PrepareExpressions(preparer);
		}
	}
}