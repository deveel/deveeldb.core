using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class DistinctNode : SingleQueryPlanNode {
		public DistinctNode(IQueryPlanNode child, ObjectName[] columnNames)
			: base(child) {
			ColumnNames = columnNames;
		}

		public ObjectName[] ColumnNames { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			return table.DistinctBy(ColumnNames);
		}
	}
}