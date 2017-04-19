using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query.Plan {
	class SimpleQuantifyExpressionPlan : ExpressionPlan {
		public SimpleQuantifyExpressionPlan(ObjectName reference, SqlExpressionType @operator, SqlExpressionType subOperator, SqlExpression expression, float optimizeFactor)
			: base(optimizeFactor) {
			Reference = reference;
			Operator = @operator;
			SubOperator = subOperator;
			Expression = expression;
		}

		public ObjectName Reference { get; }

		public SqlExpressionType Operator { get; }

		public SqlExpressionType SubOperator { get; }

		public SqlExpression Expression { get; }

		public override void AddToPlan(TableSetPlan plan) {
			var tablePlan = plan.FindTablePlan(Reference);
			tablePlan.UpdatePlan(new QuantifiedSelectNode(tablePlan.Plan,
				SqlExpression.Quantify(Operator,
					SqlExpression.Binary(SubOperator, SqlExpression.Reference(Reference), Expression))));
		}
	}
}