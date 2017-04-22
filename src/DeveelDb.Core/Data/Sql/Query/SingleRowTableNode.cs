using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class SingleRowTableNode : QueryPlanNodeBase {
		public override Task<ITable> ReduceAsync(IContext context) {
			return Task.FromResult(TemporaryTable.SingleRow);
		}
	}
}