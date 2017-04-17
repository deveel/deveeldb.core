using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class QueryPlanNodeSample {
		private List<QueryPlanNodeSample> childNodes;

		internal QueryPlanNodeSample(IQueryPlanNode node, QueryPlanNodeSampleInfo sampleInfo) {
			Node = node;
			SampleInfo = sampleInfo;
			childNodes = new List<QueryPlanNodeSample>();
		}

		private IQueryPlanNode Node { get; }

		public string NodeName => Node.NodeName;

		public QueryPlanNodeSampleInfo SampleInfo { get; }

		public IDictionary<string, object> Data => Node.Data;

		public IEnumerable<QueryPlanNodeSample> ChildNodes => childNodes.AsEnumerable();

		internal void AddChild(QueryPlanNodeSample child) {
			childNodes.Add(child);
		}
	}
}