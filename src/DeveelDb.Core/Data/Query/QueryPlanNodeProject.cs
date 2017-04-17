using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class QueryPlanNodeProject {
		public QueryPlanNodeProject(IQueryPlanNode node) {
			Node = node;
		}

		public IQueryPlanNode Node { get; }

		public string NodeName => Node.NodeName;

		public IEnumerable<QueryPlanNodeProject> ChildNodes => Node.ChildNodes.Select(x => new QueryPlanNodeProject(x));

		public async Task<QueryPlanNodeSample> SampleAsync(IContext context) {
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			ITable result;

			try {
				result = await Node.ReduceAsync(context);
			} finally {
				stopwatch.Stop();	
			}

			return new QueryPlanNodeSample(result.TableInfo, result.RowCount, stopwatch.Elapsed);
		}
	}
}