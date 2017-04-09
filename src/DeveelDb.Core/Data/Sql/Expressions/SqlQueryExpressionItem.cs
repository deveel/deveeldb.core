using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlQueryExpressionItem : ISqlFormattable {
		public SqlQueryExpressionItem(SqlExpression expression) 
			: this(expression, null) {
		}

		public SqlQueryExpressionItem(SqlExpression expression, string alias) {
			Expression = expression;
			Alias = alias;
		}

		static SqlQueryExpressionItem() {
			All = new SqlQueryExpressionItem(SqlExpression.Reference(new ObjectName("*")));
		}

		public SqlExpression Expression { get; }

		public string Alias { get; }

		public bool IsAliased => !String.IsNullOrWhiteSpace(Alias);

		public static SqlQueryExpressionItem All { get; }

		public bool IsAll => Expression is SqlReferenceExpression &&
		                     ((SqlReferenceExpression) Expression).ReferenceName.FullName == ObjectName.Glob.ToString();

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			if (IsAll) {
				builder.Append("*");
			} else {
				Expression.AppendTo(builder);

				if (IsAliased)
					builder.AppendFormat(" AS {0}", Alias);
			}
		}
	}
}