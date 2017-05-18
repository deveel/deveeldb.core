using System;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class LeftOuterJoinNode : SingleQueryPlanNode {
		public LeftOuterJoinNode(IQueryPlanNode child, string cacheKey)
			: base(child) {
			CacheKey = cacheKey;
		}

		public string CacheKey { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var cache = context.Scope.Resolve<ITableCache>();
			if (cache == null)
				throw new InvalidOperationException("No table cache was registered");

			ITable completeLeft;
			if (!cache.TryGetTable(CacheKey, out completeLeft))
				throw new InvalidOperationException($"No table with key {CacheKey} was cached");

			var left = await Child.ReduceAsync(context);
			var outside = completeLeft.Outside(left);

			return left.Outer(outside);
		}
	}
}