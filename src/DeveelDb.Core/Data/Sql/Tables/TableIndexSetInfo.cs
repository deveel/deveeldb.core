using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Tables {
	public class TableIndexSetInfo : IEnumerable<TableIndexInfo> {
		private readonly List<TableIndexInfo> indexes;

		public TableIndexSetInfo(ObjectName tableName, IEnumerable<TableIndexInfo> indexes, bool readOnly) {
			TableName = tableName;
			ReadOnly = readOnly;

			this.indexes = new List<TableIndexInfo>();

			if (indexes != null)
				this.indexes.AddRange(indexes);
		}

		public ObjectName TableName { get; }

		public bool ReadOnly { get; }

		public int IndexCount => indexes.Count;

		public IEnumerator<TableIndexInfo> GetEnumerator() {
			return indexes.GetEnumerator();
		}

		public TableIndexInfo GetIndex(int offset) {
			return indexes.FirstOrDefault(x => x.Offset == offset);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}