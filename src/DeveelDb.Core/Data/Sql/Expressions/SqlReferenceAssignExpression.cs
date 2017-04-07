using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlReferenceAssignExpression : SqlExpression {
		internal SqlReferenceAssignExpression(ObjectName referenceName, SqlExpression value)
			: base(SqlExpressionType.ReferenceAssign) {
			if (referenceName == null)
				throw new ArgumentNullException(nameof(referenceName));
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			ReferenceName = referenceName;
			Value = value;
		}

		public ObjectName ReferenceName { get; }

		public SqlExpression Value { get; }

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitReferenceAssign(this);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			ReferenceName.AppendTo(builder);
			builder.Append(" = ");
			Value.AppendTo(builder);
		}
	}
}