using System;

namespace Deveel.Data.Sql.Expressions {
	static class SqlExpressionTypeExtensions {
		public static bool IsBinary(this SqlExpressionType expressionType) {
			switch (expressionType) {
				case SqlExpressionType.Add:
				case SqlExpressionType.Subtract:
				case SqlExpressionType.Divide:
				case SqlExpressionType.Multiply:
				case SqlExpressionType.And:
				case SqlExpressionType.Or:
				case SqlExpressionType.XOr:
				case SqlExpressionType.Equal:
				case SqlExpressionType.NotEqual:
				case SqlExpressionType.GreaterThan:
				case SqlExpressionType.GreaterThanOrEqual:
				case SqlExpressionType.LessThan:
				case SqlExpressionType.LessThanOrEqual:
					return true;
				default:
					return false;
			}
		}

		public static bool IsUnary(this SqlExpressionType expressionType) {
			switch (expressionType) {
				case SqlExpressionType.UnaryPlus:
				case SqlExpressionType.Not:
				case SqlExpressionType.Negate:
					return true;
				default:
					return false;
			}
		}
	}
}