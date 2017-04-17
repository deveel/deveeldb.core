using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class LimitResultNode : SingleQueryPlanNode {
		public LimitResultNode(IQueryPlanNode child, long offset, long count)
			: base(child) {
			Offset = offset;
			Count = count;
		}

		public long Offset { get; }

		public long Count { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			return table.Limit(Offset, Count);
		}
	}
}