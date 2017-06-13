using System;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	struct TableStateInfo {
		public TableStateInfo(int id, string sourceName, string systemId) {
			Id = id;
			SourceName = sourceName;
			SystemId = systemId;
		}

		public string SourceName { get; }

		public string SystemId { get; }

		public int Id { get; }
	}
}