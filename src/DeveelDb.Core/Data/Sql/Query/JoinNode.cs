using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query {
	public sealed class JoinNode : BranchQueryPlanNode {
		public JoinNode(IQueryPlanNode left, IQueryPlanNode right, ObjectName leftColumnName, SqlExpressionType op, SqlExpression rightExpression)
			: base(left, right) {
			LeftColumnName = leftColumnName;
			Operator = op;
			RightExpression = rightExpression;
		}

		public ObjectName LeftColumnName { get; }

		public SqlExpressionType Operator { get; }

		public SqlExpression RightExpression { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var left = await Left.ReduceAsync(context);
			var right = await Right.ReduceAsync(context);

			return await left.JoinAsync(context, right, LeftColumnName, Operator, RightExpression);
		}
	}
}