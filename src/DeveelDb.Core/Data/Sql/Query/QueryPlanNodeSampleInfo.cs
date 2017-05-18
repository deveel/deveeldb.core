using System;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class QueryPlanNodeSampleInfo {
		internal QueryPlanNodeSampleInfo(TableInfo tableInfo, long rowCount, TimeSpan executionTime) {
			TableInfo = tableInfo;
			RowCount = rowCount;
			ExecutionTime = executionTime;
		}

		public TableInfo TableInfo { get; }

		public long RowCount { get; }

		public TimeSpan ExecutionTime { get; }
	}
}