using System;
using System.Collections;
using System.Collections.Generic;

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

		public IEnumerator<TableIndexInfo> GetEnumerator() {
			return indexes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}