using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query.Plan {
	class ComplexSingleExpressionPlan : ExpressionPlan {
		public ComplexSingleExpressionPlan(ObjectName reference, SqlExpression expression, float optimizeFactor) 
			: base(optimizeFactor) {
			Reference = reference;
			Expression = expression;
		}

		public ObjectName Reference { get; }

		public SqlExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			var tablePlan = plan.FindTablePlan(Reference);
			tablePlan.UpdatePlan(new FullSelectNode(tablePlan.Plan, Expression));
		}
	}
}