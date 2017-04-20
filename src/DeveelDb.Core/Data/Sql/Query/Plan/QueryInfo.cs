using System;
using System.Collections.Generic;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	public sealed class QueryInfo {
		public QueryInfo(SqlQueryExpression query, IEnumerable<SortColumn> sortColumns, QueryLimit limit) {
			Query = query;
			SortColumns = sortColumns;
			Limit = limit;
		}

		public SqlQueryExpression Query { get; }

		public IEnumerable<SortColumn> SortColumns { get; }

		public QueryLimit Limit { get; }
	}
}