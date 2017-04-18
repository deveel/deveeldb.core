using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	static class QueryPlanSampler {
		public static async Task<QueryPlanNodeSample> SampleAsync(IContext context, IQueryPlanNode queryPlan) {
			var result = await SampleNodeAsync(context, queryPlan);
			var sample = new QueryPlanNodeSample(queryPlan, result);

			if (queryPlan.ChildNodes != null && queryPlan.ChildNodes.Length > 0) {
				foreach (var childNode in queryPlan.ChildNodes) {
					var childResult = await SampleAsync(context, childNode);
					sample.AddChild(childResult);
				}
			}

			return sample;
		}

		private static async Task<QueryPlanNodeSampleInfo> SampleNodeAsync(IContext context, IQueryPlanNode node) {
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			ITable result;

			try {
				result = await node.ReduceAsync(context);
			} finally {
				stopwatch.Stop();
			}

			return new QueryPlanNodeSampleInfo(result.TableInfo, result.RowCount, stopwatch.Elapsed);
		}

	}
}