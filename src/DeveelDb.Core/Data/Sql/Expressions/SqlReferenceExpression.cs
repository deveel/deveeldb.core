using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlReferenceExpression : SqlExpression {
		internal SqlReferenceExpression(ObjectName reference)
			: base(SqlExpressionType.Reference) {
			if (reference == null)
				throw new ArgumentNullException(nameof(reference));

			ReferenceName = reference;
		}

		public ObjectName ReferenceName { get; }

		protected override void AppendTo(SqlStringBuilder builder) {
			ReferenceName.AppendTo(builder);
		}

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitReference(this);
		}
	}
}