using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query.Plan {
	class ExhaustiveSelectExpressionPlan: ExpressionPlan {
		public ExhaustiveSelectExpressionPlan(SqlExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Expression = expression;
		}

		public SqlExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			throw new NotImplementedException();
		}
	}
}