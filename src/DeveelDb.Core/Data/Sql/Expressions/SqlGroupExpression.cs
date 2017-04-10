using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlGroupExpression : SqlExpression {
		internal SqlGroupExpression(SqlExpression expression)
			: base(SqlExpressionType.Group) {
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			Expression = expression;
		}

		public SqlExpression Expression { get; }

		public override SqlType GetSqlType(IContext context) {
			return Expression.GetSqlType(context);
		}

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitGroup(this);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("(");
			Expression.AppendTo(builder);
			builder.Append(")");
		}
	}
}