// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
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