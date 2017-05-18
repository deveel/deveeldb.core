using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class ExhaustiveJoinExpressionPlan : ExpressionPlan {
		public ExhaustiveJoinExpressionPlan(SqlBinaryExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Expression = expression;
		}

		public SqlBinaryExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			var allRefs = Expression.DiscoverReferences();
			var allPlan =plan.JoinAllPlansWithReferences(allRefs);

			allPlan.UpdatePlan(new FullSelectNode(allPlan.Plan, Expression));
		}
	}
}