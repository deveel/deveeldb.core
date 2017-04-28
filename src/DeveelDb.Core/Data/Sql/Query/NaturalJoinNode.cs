using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class NaturalJoinNode : BranchQueryPlanNode {
		public NaturalJoinNode(IQueryPlanNode left, IQueryPlanNode right) 
			: base(left, right) {
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var left = await Left.ReduceAsync(context);
			var right = await Right.ReduceAsync(context);

			return left.NaturalJoin(right);
		}
	}
}