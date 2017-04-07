using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlConstantExpression : SqlExpression {
		public SqlConstantExpression(SqlObject value) 
			: base(SqlExpressionType.Constant) {
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			Value = value;
		}

		public SqlObject Value { get; }

		protected override void AppendTo(SqlStringBuilder builder) {
			(Value as ISqlFormattable).AppendTo(builder);
		}
	}
}