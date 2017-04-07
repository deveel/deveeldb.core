using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlCastExpression : SqlExpression {
		internal SqlCastExpression(SqlExpression value, SqlType targetType)
			: base(SqlExpressionType.Cast) {
			if (value == null)
				throw new ArgumentException(nameof(value));
			if (targetType == null)
				throw new ArgumentNullException(nameof(targetType));

			Value = value;
			TargetType = targetType;
		}

		public SqlExpression Value { get; }

		public SqlType TargetType { get; }

		public override bool CanReduce => true;

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitCast(this);
		}

		public override SqlExpression Reduce(IContext context) {
			var value = Value.Reduce(context);

			if (!(value is SqlConstantExpression))
				throw new SqlExpressionException("The value of the cast could not be reduced to constant");

			var obj = ((SqlConstantExpression) value).Value;
			var result = obj.CastTo(TargetType);

			return Constant(result);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("CAST(");
			Value.AppendTo(builder);
			builder.Append(" AS ");
			TargetType.AppendTo(builder);
			builder.Append(")");
		}
	}
}