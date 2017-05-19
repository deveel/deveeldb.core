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

namespace Deveel.Data.Sql.Tables {
	public class InMemoryTableCache : ITableCache {
		private Dictionary<string, ITable> tables;

		public InMemoryTableCache() {
			tables = new Dictionary<string, ITable>();
		}

		public bool TryGetTable(string key, out ITable table) {
			// TODO: notify to the listeners that we are getting from cache?
			return tables.TryGetValue(key, out table);
		}

		public void SetTable(string key, ITable table) {
			// TODO: notify to the listeners that we are setting to cache?
			tables[key] = table;
		}

		public void Clear() {
			tables.Clear();
		}
	}
}