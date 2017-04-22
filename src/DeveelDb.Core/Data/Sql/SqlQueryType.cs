using System;

using Deveel.Data.Sql.Query;

namespace Deveel.Data.Sql {
	public sealed class SqlQueryType : SqlType {
		internal SqlQueryType()
			: base(SqlTypeCode.QueryPlan) {
		}

		public override bool IsComparable(SqlType type) {
			return false;
		}

		public override bool IsInstanceOf(ISqlValue value) {
			return value is IQueryPlanNode;
		}
	}
}