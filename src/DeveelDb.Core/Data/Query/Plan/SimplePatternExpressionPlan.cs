using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query.Plan {
	class SimplePatternExpressionPlan : ExpressionPlan {
		public SimplePatternExpressionPlan(ObjectName reference, SqlExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			
		}

		public override void AddToPlan(TableSetPlan plan) {
			throw new NotImplementedException();
		}
	}
}