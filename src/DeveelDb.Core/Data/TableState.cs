using System;

namespace Deveel.Data {
	struct TableState {
		public TableState(int tableId, string sourceName) {
			TableId = tableId;
			SourceName = sourceName;
		}

		public int TableId { get; }

		public string SourceName { get; }
	}
}