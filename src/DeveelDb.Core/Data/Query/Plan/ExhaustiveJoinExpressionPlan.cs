using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query.Plan {
	class ExhaustiveJoinExpressionPlan : ExpressionPlan {
		public ExhaustiveJoinExpressionPlan(SqlBinaryExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Expression = expression;
		}

		public SqlBinaryExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			throw new NotImplementedException();
		}
	}
}