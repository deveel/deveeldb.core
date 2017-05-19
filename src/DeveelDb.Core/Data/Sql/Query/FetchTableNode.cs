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
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class FetchTableNode : QueryPlanNodeBase {
		public FetchTableNode(ObjectName tableName) : this(tableName, null) {
		}

		public FetchTableNode(ObjectName tableName, ObjectName alias) {
			TableName = tableName;
			Alias = alias;
		}

		public ObjectName TableName { get; }

		public ObjectName Alias { get; }

		protected override void GetData(IDictionary<string, object> data) {
			data["table"] = TableName;
			data["alias"] = Alias;
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await context.GetTableAsync(TableName);
			if (Alias != null)
				table = new AliasedTable(table, Alias);

			return table;
		}
	}
}