using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlQueryExpressionComposite : ISqlExpressionPreparable<SqlQueryExpressionComposite>, ISqlFormattable {
		public SqlQueryExpressionComposite(CompositeFunction function, SqlQueryExpression expression) 
			: this(function, false, expression) {
		}

		public SqlQueryExpressionComposite(CompositeFunction function, bool all, SqlQueryExpression expression) {
			Function = function;
			All = all;
			Expression = expression;
		}

		public SqlQueryExpression Expression { get; }

		public CompositeFunction Function { get; }

		public bool All { get; }

		SqlQueryExpressionComposite ISqlExpressionPreparable<SqlQueryExpressionComposite>.Prepare(ISqlExpressionPreparer preparer) {
			var expression = (SqlQueryExpression) Expression.Prepare(preparer);
			return new SqlQueryExpressionComposite(Function, All, expression);
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			builder.Append(Function.ToString().ToUpperInvariant());
			builder.Append(" ");

			if (All)
				builder.Append("ALL ");

			Expression.AppendTo(builder);
		}

	}
}