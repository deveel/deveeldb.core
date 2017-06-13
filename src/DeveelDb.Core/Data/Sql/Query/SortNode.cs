﻿// 
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
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class SortNode : SingleQueryPlanNode {
		public SortNode(IQueryPlanNode child, ObjectName[] columns, bool[] ascending)
			: base(child) {
			Columns = columns;
			Ascending = @ascending;
		}

		public ObjectName[] Columns { get; }

		public bool[] Ascending { get; }

		protected override void GetData(IDictionary<string, object> data) {
			for (int i = 0; i < Columns.Length; i++) {
				data[Columns[i].FullName] = Ascending[i] ? "ASC" : "DESC";
			}
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			return table.OrderBy(Columns, Ascending);
		}
	}
}