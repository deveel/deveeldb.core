using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class SimpleSubQueryExpressionPlan : ExpressionPlan {
		public SimpleSubQueryExpressionPlan(SqlQuantifyExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Expression = expression;
		}

		public SqlQuantifyExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			var op = Expression.ExpressionType;
			var subOp = Expression.Expression.ExpressionType;
			var leftRef = Expression.Expression.Left.AsReference();
			var rightPlan = Expression.Expression.Right.AsQueryPlanNode();

			var tablePlan = plan.FindTablePlan(leftRef);
			var leftPlan = tablePlan.Plan;

			tablePlan.UpdatePlan(new NonCorrelatedSelectNode(leftPlan, rightPlan, new []{leftRef}, op, subOp));
		}
	}
}