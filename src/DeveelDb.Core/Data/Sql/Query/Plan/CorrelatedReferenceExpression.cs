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

		public SqlConstantExpression Value { get; }

		public override SqlType GetSqlType(IContext context) {
			return Value.Type;
		}

		public override async Task<SqlExpression> ReduceAsync(IContext context) {
			var refExp = Reference(ReferenceName);
			var value = await refExp.ReduceToConstantAsync(context);
			return new CorrelatedReferenceExpression(ReferenceName, QueryLevel, Constant(value));
		}
	}
}