using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlUnaryExpression : SqlExpression {
		internal SqlUnaryExpression(SqlExpressionType expressionType, SqlExpression operand)
			: base(expressionType) {
			if (operand == null)
				throw new ArgumentNullException(nameof(operand));

			Operand = operand;
		}

		public SqlExpression Operand { get; }

		public override bool CanReduce => true;

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitUnary(this);
		}

		public override SqlExpression Reduce(IContext context) {
			var operand = Operand.Reduce(context);
			if (operand.ExpressionType != SqlExpressionType.Constant)
				throw new SqlExpressionException("Operand of a unary operator could not be reduced to a constant.");

			var result = ReduceUnary(((SqlConstantExpression) operand).Value);
			return Constant(result);
		}

		private SqlObject ReduceUnary(SqlObject value) {
			switch (ExpressionType) {
				case SqlExpressionType.UnaryPlus:
					return value.Plus();
				case SqlExpressionType.Negate:
					return value.Negate();
				case SqlExpressionType.Not:
					return value.Not();
				default:
					throw new SqlExpressionException($"Expression of type {ExpressionType} is not unary.");
			}
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append(GetUnaryOperator());
			Operand.AppendTo(builder);
		}

		private string GetUnaryOperator() {
			switch (ExpressionType) {
				case SqlExpressionType.Negate:
					return "-";
				case SqlExpressionType.Not:
					return "~";
				case SqlExpressionType.UnaryPlus:
					return "+";
				default:
					throw new SqlExpressionException($"Expression type {ExpressionType} has no operator");
			}
		}
	}
}