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

using Deveel.Data.Services;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class CacheNode : SingleQueryPlanNode {
		private static readonly object globalLock = new object();
		private static int globalId;

		public CacheNode(IQueryPlanNode child) 
			: this(child, NewId()) {
		}

		public CacheNode(IQueryPlanNode child, long id) 
			: base(child) {
			Id = id;
		}

		public long Id { get; }

		private static long NewId() {
			long id;
			lock (globalLock) {
				id = ((int)DateTime.Now.Ticks << 16) | (globalId & 0x0FFFF);
				++globalId;
			}
			return id;
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var cache = context.Scope.Resolve<ITableCache>();
			if (cache == null)
				throw new InvalidOperationException("No table cache was found in context");

			ITable table;
			if (!cache.TryGetTable(Id.ToString(), out table)) {
				table = await Child.ReduceAsync(context);
				cache.SetTable(Id.ToString(), table);
			}

			return table;
		}
	}
}