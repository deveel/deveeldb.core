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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
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