using System;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class CacheNode : SingleQueryPlanNode {
		private static readonly object globalLock = new object();
		private static int globalId;

		public CacheNode(IQueryPlanNode child) 
			: this(child, NewId()) {
		}

		public CacheNode(IQueryPlanNode child, long id) 
			: base(child) {
			Id = id;
		}

		public long Id { get; }

		private static long NewId() {
			long id;
			lock (globalLock) {
				id = ((int)DateTime.Now.Ticks << 16) | (globalId & 0x0FFFF);
				++globalId;
			}
			return id;
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var cache = context.Scope.Resolve<ITableCache>();
			if (cache == null)
				throw new InvalidOperationException("No table cache was found in context");

			ITable table;
			if (!cache.TryGetTable(Id.ToString(), out table)) {
				table = await Child.ReduceAsync(context);
				cache.SetTable(Id.ToString(), table);
			}

			return table;
		}
	}
}