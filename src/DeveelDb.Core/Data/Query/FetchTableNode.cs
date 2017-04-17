using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class FetchTableNode : QueryPlanNodeBase {
		public FetchTableNode(ObjectName tableName) {
			TableName = tableName;
		}

		public ObjectName TableName { get; }

		public override Task<ITable> ReduceAsync(IContext context) {
			return context.GetTableAsync(TableName);
		}
	}
}