using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql {
	/// <summary>
	/// Object used to represent a column in the <c>ORDER BY</c> clauses 
	/// of a select statement.
	/// </summary>
	public sealed class SortColumn : ISqlExpressionPreparable<SortColumn>, ISqlFormattable {
		/// <summary>
		/// Constructs the <c>BY</c> column reference with the expression
		/// and the sort order given.
		/// </summary>
		/// <param name="expression">The expression of the column reference.</param>
		/// <param name="ascending">The sort order for the column. If this is
		/// set to <b>true</b>, the column will be used to sort the results of
		/// a query in ascending order.</param>
		public SortColumn(SqlExpression expression, bool ascending) {
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			Expression = expression;
			Ascending = ascending;
		}

		/// <summary>
		/// Constructs the <c>BY</c> column reference with the expression
		/// given and the ascending sort order.
		/// </summary>
		/// <param name="expression">The expression of the column reference.</param>
		public SortColumn(SqlExpression expression)
			: this(expression, true) {
		}

		/// <summary>
		/// Gets the expression used to order the result of a query.
		/// </summary>
		public SqlExpression Expression { get; internal set; }

		/// <summary>
		/// Gets a boolean value indicating whether we're sorting in ascending
		/// or descending order.
		/// </summary>
		public bool Ascending { get; private set; }

		SortColumn ISqlExpressionPreparable<SortColumn>.Prepare(ISqlExpressionPreparer preparer) {
			var exp = Expression;
			if (exp != null) {
				exp = SqlExpressionPreparerExtensions.Prepare(exp, preparer);
			}

			return new SortColumn(exp, Ascending);
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			Expression.AppendTo(builder);
			builder.Append(" ");

			if (Ascending) {
				builder.Append("ASC");
			} else {
				builder.Append("DESC");
			}
		}
	}
}
