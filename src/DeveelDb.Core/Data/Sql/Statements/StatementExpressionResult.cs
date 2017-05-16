using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public sealed class StatementExpressionResult : IStatementResult {
		public StatementExpressionResult(SqlExpression value) {
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			Value = value;
		}

		public SqlExpression Value { get; }
	}
}