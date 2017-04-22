using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Query {
	public static class QueryNodeExtensions {
		public static Task<QueryPlanNodeSample> SampleAsync(this IQueryPlanNode queryPlan, IContext context) {
			return QueryPlanSampler.SampleAsync(context, queryPlan);
		}
	}
}