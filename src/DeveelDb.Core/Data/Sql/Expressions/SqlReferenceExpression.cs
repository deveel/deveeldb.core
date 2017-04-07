using System;

using Deveel.Data.Services;

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

		public override bool CanReduce => true;

		public override SqlExpression Reduce(IContext context) {
			if (context == null)
				throw new SqlExpressionException("A reference cannot be reduced outside a context.");

			var resolver = context.Scope.Resolve<IReferenceResolver>();
			if (resolver == null)
				throw new SqlExpressionException("No reference resolver was declared in this scope");

			var value = resolver.ResolveReference(ReferenceName);
			if (value == null)
				value = SqlObject.Unknown;

			return Constant(value);
		}
	}
}