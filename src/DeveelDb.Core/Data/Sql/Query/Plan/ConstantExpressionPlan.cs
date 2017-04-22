using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class ConstantExpressionPlan : ExpressionPlan {
		public SqlExpression Expression { get; }

		public ConstantExpressionPlan(SqlExpression expression)
			: base(0f) {
			Expression = expression;
		}

		public override void AddToPlan(TableSetPlan plan) {
			var tablePlan = plan.GetTablePlan(0);
			tablePlan.UpdatePlan(new ConstantSelectNode(tablePlan.Plan, Expression));
		}
	}
}