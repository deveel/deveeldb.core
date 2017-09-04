using System;

namespace Deveel.Data.Sql.Tables {
	public class TableIndexInfo {
		public TableIndexInfo(ObjectName indexName, int offset) {
			IndexName = indexName;
			Offset = offset;
		}

		public ObjectName IndexName { get; }

		public int Offset { get; }
	}
}