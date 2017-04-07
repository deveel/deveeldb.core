using System;

using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Expressions {
	class SqlTypeExpressionVisitor : SqlExpressionVisitor {
		private readonly IContext context;

		public SqlTypeExpressionVisitor(IContext context) {
			this.context = context;
		}

		public SqlType Type { get; private set; }

		public override SqlExpression VisitConstant(SqlConstantExpression constant) {
			Type = constant.Value.Type;
			return base.VisitConstant(constant);
		}

		public override SqlExpression VisitCast(SqlCastExpression expression) {
			Type = expression.TargetType;
			return base.VisitCast(expression);
		}

		public override SqlExpression VisitVariable(SqlVariableExpression expression) {
			var variable = context.ResolveVariable(expression.VariableName);
			if (variable != null)
				Type = variable.Type;

			return base.VisitVariable(expression);
		}

		public override SqlExpression VisitVariableAssign(SqlVariableAssignExpression expression) {
			var variable = context.ResolveVariable(expression.VariableName);
			if (variable != null)
				Type = variable.Type;

			return base.VisitVariableAssign(expression);
		}
	}
}