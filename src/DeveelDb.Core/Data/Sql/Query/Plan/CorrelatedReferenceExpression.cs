using System;
using System.Threading.Tasks;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class CorrelatedReferenceExpression : SqlExpression {
		public CorrelatedReferenceExpression(ObjectName reference, int queryLevel)
			: this(reference, queryLevel, null) {
		}

		public CorrelatedReferenceExpression(ObjectName reference, int queryLevel, SqlConstantExpression value)
			: base((SqlExpressionType) 233) {
			ReferenceName = reference;
			QueryLevel = queryLevel;
			Value = value;
		}

		public ObjectName ReferenceName { get; }

		public int QueryLevel { get; }

		public SqlConstantExpression Value { get; set; }

		public override bool IsReference => true;

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return this;
		}

		public override SqlType GetSqlType(IContext context) {
			if (Value != null)
				return Value.GetSqlType(context);

			var resolver = context.GetReferenceResolver();
			if (resolver == null)
				throw new SqlExpressionException();

			return resolver.ResolveType(ReferenceName);
		}

		public override async Task<SqlExpression> ReduceAsync(IContext context) {
			if (Value != null)
				return Value;

			var resolver = context.GetReferenceResolver();
			if (resolver == null)
				throw new SqlExpressionException();

			var value = await resolver.ResolveReferenceAsync(ReferenceName);
			return Constant(value);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			ReferenceName.AppendTo(builder);
			builder.AppendFormat("({0})", QueryLevel);
		}
	}
}