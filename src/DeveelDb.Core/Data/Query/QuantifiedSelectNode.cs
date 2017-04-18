using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class QuantifiedSelectNode : SingleQueryPlanNode {
		public QuantifiedSelectNode(IQueryPlanNode child, SqlQuantifyExpression expression)
			: base(child) {
			Expression = expression;
		} 

		public SqlQuantifyExpression Expression { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);
			return await table.SelectNonCorrelatedAsync(context, Expression);
		}
	}
}