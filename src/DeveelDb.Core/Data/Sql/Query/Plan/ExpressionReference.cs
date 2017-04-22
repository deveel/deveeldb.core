using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class ExpressionReference {
		public ExpressionReference(SqlExpression expression, string @alias) {
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			Alias = alias;
			Expression = expression;
		}

		public SqlExpression Expression { get; }

		public string Alias { get; }
	}
}