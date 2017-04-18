using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query.Plan {
	class ConstantExpressionPlan : ExpressionPlan {
		public SqlExpression Expression { get; }

		public ConstantExpressionPlan(SqlExpression expression)
			: base(0f) {
			Expression = expression;
		}

		public override void AddToPlan(TableSetPlan plan) {
			throw new NotImplementedException();
		}
	}
}