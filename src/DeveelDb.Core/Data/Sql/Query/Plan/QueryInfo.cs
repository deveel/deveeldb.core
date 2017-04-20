using System;
using System.Collections.Generic;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	public sealed class QueryInfo {
		public QueryInfo(SqlQueryExpression query, QueryLimit limit) : this(query, new SortColumn[0], limit) {
		}

		public QueryInfo(SqlQueryExpression query) 
			: this(query, new SortColumn[0]) {
		}

		public QueryInfo(SqlQueryExpression query, IEnumerable<SortColumn> sortColumns) 
			: this(query, sortColumns, null) {
		}

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