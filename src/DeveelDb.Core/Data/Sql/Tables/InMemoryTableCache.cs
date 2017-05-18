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