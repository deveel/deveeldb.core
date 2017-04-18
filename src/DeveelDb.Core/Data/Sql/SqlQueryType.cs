using System;

namespace Deveel.Data.Sql {
	public sealed class SqlQueryType : SqlType {
		internal SqlQueryType()
			: base(SqlTypeCode.QueryPlan) {
		}

		public override bool IsComparable(SqlType type) {
			return false;
		}
	}
}