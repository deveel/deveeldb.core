using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class CacheMarkNode : SingleQueryPlanNode {
		public CacheMarkNode(IQueryPlanNode child, string markerName)
			: base(child) {
			MarkerName = markerName;
		}

		public string MarkerName { get; }

		protected override void GetData(IDictionary<string, object> data) {
			data["mark"] = MarkerName;
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			var cache = context.ResolveService<ITableCache>();

			cache.SetTable(MarkerName, table);

			return table;
		}
	}
}