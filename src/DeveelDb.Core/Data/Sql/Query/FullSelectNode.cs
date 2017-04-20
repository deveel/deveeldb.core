using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class FullSelectNode : SingleQueryPlanNode {
		public FullSelectNode(IQueryPlanNode child, SqlExpression expression)
			: base(child) {
			Expression = expression;
		}

		public SqlExpression Expression { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			return await table.FullSelectAsync(context, Expression);
		}
	}
}