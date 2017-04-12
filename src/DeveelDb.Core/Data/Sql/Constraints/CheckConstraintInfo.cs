using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Constraints {
	public sealed class CheckConstraintInfo : ConstraintInfo {
		public CheckConstraintInfo(string constraintName, ObjectName tableName, SqlExpression expression) 
			: base(constraintName, ConstraintType.Check, tableName) {
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			Expression = expression;
		}

		public SqlExpression Expression { get; }
	}
}