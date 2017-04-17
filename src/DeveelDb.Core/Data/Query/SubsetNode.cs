using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class SubsetNode : SingleQueryPlanNode {
		public SubsetNode(IQueryPlanNode child, ObjectName[] columnNames, ObjectName[] aliases)
			: base(child) {
			ColumnNames = columnNames;
			Aliases = aliases;
		}

		public ObjectName[] ColumnNames { get; }

		public ObjectName[] Aliases { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			return table.Subset(ColumnNames, Aliases);
		}
	}
}