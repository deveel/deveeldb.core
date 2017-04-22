using System;

namespace Deveel.Data.Sql.Query {
	public abstract class SingleQueryPlanNode : QueryPlanNodeBase {
		protected SingleQueryPlanNode(IQueryPlanNode child) {
			Child = child;
		}

		public IQueryPlanNode Child { get; }

		protected override IQueryPlanNode[] ChildNodes => new[] {Child};
	}
}