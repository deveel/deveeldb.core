﻿using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class SubLogicExpressionPlan : ExpressionPlan {
		public SubLogicExpressionPlan(SqlBinaryExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Expression = expression;
		}

		public SqlBinaryExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			plan.PlanForExpression(Expression);
		}
	}
}