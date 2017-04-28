using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class SortNode : SingleQueryPlanNode {
		public SortNode(IQueryPlanNode child, ObjectName[] columns, bool[] ascending)
			: base(child) {
			Columns = columns;
			Ascending = @ascending;
		}

		public ObjectName[] Columns { get; }

		public bool[] Ascending { get; }

		protected override void GetData(IDictionary<string, object> data) {
			for (int i = 0; i < Columns.Length; i++) {
				data[Columns[i].FullName] = Ascending[i] ? "ASC" : "DESC";
			}
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			return table.OrderBy(Columns, Ascending);
		}
	}
}