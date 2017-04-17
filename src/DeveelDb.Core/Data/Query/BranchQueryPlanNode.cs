using System;

namespace Deveel.Data.Query {
	public abstract class BranchQueryPlanNode : QueryPlanNodeBase {
		protected BranchQueryPlanNode(IQueryPlanNode left, IQueryPlanNode right) {
			Left = left;
			Right = right;
		}

		public IQueryPlanNode Left { get; }

		public IQueryPlanNode Right { get; }

		protected override IQueryPlanNode[] ChildNodes => new[] {Left, Right};
	}
}