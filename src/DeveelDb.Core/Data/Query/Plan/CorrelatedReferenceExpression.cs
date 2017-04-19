using System;
using System.Threading.Tasks;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Query.Plan {
	class CorrelatedReferenceExpression : SqlExpression {
		public CorrelatedReferenceExpression(ObjectName reference, int queryLevel)
			: this(reference, queryLevel, null) {
		}

		public CorrelatedReferenceExpression(ObjectName reference, int queryLevel, SqlConstantExpression value)
			: base((SqlExpressionType) 233) {
			Reference = reference;
			QueryLevel = queryLevel;
			Value = value;
		}

		public ObjectName Reference { get; }

		public int QueryLevel { get; }

		public SqlConstantExpression Value { get; }

		public override SqlType GetSqlType(IContext context) {
			return Value.Type;
		}

		public override async Task<SqlExpression> ReduceAsync(IContext context) {
			var refExp = Reference(Reference);
			var value = await refExp.ReduceToConstantAsync(context);
			return new CorrelatedReferenceExpression(Reference, QueryLevel, Constant(value));
		}
	}
}