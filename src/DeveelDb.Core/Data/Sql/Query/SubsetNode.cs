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
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class SubsetNode : SingleQueryPlanNode {
		public SubsetNode(IQueryPlanNode child, ObjectName[] columnNames)
			: this(child, columnNames, columnNames) {
		}

		public SubsetNode(IQueryPlanNode child, ObjectName[] columnNames, ObjectName[] aliases)
			: base(child) {
			ColumnNames = columnNames;
			Aliases = aliases;
		}

		public ObjectName[] ColumnNames { get; }

		public ObjectName[] Aliases { get; private set; }

		public void SetAliasParentName(ObjectName parentName) {
			if (parentName != null) {
				var aliases = new ObjectName[Aliases.Length];
				for (int i = 0; i < aliases.Length; i++) {
					aliases[i] = new ObjectName(parentName, Aliases[i].Name);
				}

				Aliases = aliases;
			}
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			return table.Subset(ColumnNames, Aliases);
		}
	}
}