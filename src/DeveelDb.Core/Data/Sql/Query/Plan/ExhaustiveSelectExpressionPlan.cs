using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class ExhaustiveSelectExpressionPlan: ExpressionPlan {
		public ExhaustiveSelectExpressionPlan(SqlExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Expression = expression;
		}

		public SqlExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			var allRefs = Expression.DiscoverReferences();
			var allPlan = plan.JoinAllPlansWithReferences(allRefs);
			allPlan.UpdatePlan(new FullSelectNode(allPlan.Plan, Expression));
		}
	}
}