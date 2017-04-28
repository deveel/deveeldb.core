using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class SingleRefPlan {
		public ObjectName SingleRef { get; set; }

		public ObjectName Column { get; set; }

		public SqlExpression Expression { get; set; }

		public TablePlan Table { get; set; }
	}
}