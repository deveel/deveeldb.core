using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlBinaryExpression : SqlExpression {
		internal SqlBinaryExpression(SqlExpressionType expressionType, SqlExpression left, SqlExpression right)
			: base(expressionType) {
			if (left == null)
				throw new ArgumentNullException(nameof(left));
			if (right == null)
				throw new ArgumentNullException(nameof(right));

			Left = left;
			Right = right;
		}

		public SqlExpression Left { get; }

		public SqlExpression Right { get; }

		public override bool CanReduce => true;
	}
}