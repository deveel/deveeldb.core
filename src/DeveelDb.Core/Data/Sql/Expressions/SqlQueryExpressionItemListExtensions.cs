using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlQueryExpressionItemListExtensions {
		public static void Add(this ICollection<SqlQueryExpressionItem> list, SqlExpression expression, string alias) {
			list.Add(new SqlQueryExpressionItem(expression, alias));
		}

		public static void Add(this ICollection<SqlQueryExpressionItem> list, SqlExpression expression)
			=> list.Add(expression, null);
	}
}