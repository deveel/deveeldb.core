using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class SimplePatternExpressionPlan : ExpressionPlan {
		public SimplePatternExpressionPlan(ObjectName reference, SqlExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Reference = reference;
			Expression = expression;
		}

		public ObjectName Reference { get; }

		public SqlExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			var tablePlan = plan.FindTablePlan(Reference);
			tablePlan.UpdatePlan(new SimplePatternSelectNode(tablePlan.Plan, Expression));
		}
	}
}