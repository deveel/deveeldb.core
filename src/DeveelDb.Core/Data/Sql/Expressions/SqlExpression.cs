using System;

namespace Deveel.Data.Sql.Expressions {
	public abstract class SqlExpression : ISqlFormattable {
		protected SqlExpression(SqlExpressionType expressionType) {
			ExpressionType = expressionType;
			Precedence = GetPrecedence();
		}

		private int GetPrecedence() {
			switch (ExpressionType) {
				// Primary
				case SqlExpressionType.Reference:
				case SqlExpressionType.Invoke:
				case SqlExpressionType.Constant:
				case SqlExpressionType.VariableReference:
				case SqlExpressionType.Parameter:
					return 150;

				// Unary
				case SqlExpressionType.UnaryPlus:
				case SqlExpressionType.Negate:
				case SqlExpressionType.Not:
					return 140;

				// Cast
				case SqlExpressionType.Cast:
					return 139;

				// Multiplicative
				case SqlExpressionType.Multiply:
				case SqlExpressionType.Divide:
				case SqlExpressionType.Modulo:
					return 130;

				// Additive
				case SqlExpressionType.Add:
				case SqlExpressionType.Subtract:
					return 120;

				// Relational
				case SqlExpressionType.GreaterThan:
				case SqlExpressionType.GreaterThanOrEqual:
				case SqlExpressionType.LessThan:
				case SqlExpressionType.LessThanOrEqual:
				case SqlExpressionType.Is:
				case SqlExpressionType.IsNot:
				case SqlExpressionType.Like:
				case SqlExpressionType.NotLike:
					return 110;

				// Equality
				case SqlExpressionType.Equal:
				case SqlExpressionType.NotEqual:
					return 100;
				
				// Logical
				case SqlExpressionType.And:
					return 90;
				case SqlExpressionType.Or:
					return 89;
				case SqlExpressionType.XOr:
					return 88;

				// Conditional
				case SqlExpressionType.Conditional:
					return 80;
				
				// Assign
				case SqlExpressionType.Assign:
					return 70;
				
				// Tuple
				case SqlExpressionType.Tuple:
					return 60;
			}

			return -1;
		}

		public virtual bool CanReduce {
			get { return false; }
		}

		public SqlExpressionType ExpressionType { get; }

		internal int Precedence { get; }

		public override string ToString() {
			return this.ToSqlString();
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		protected virtual void AppendTo(SqlStringBuilder builder) {
			
		}

		#region Factories

		public static SqlConstantExpression Constant(SqlObject value) {
			return new SqlConstantExpression(value);
		}

		public static SqlBinaryExpression Binary(SqlExpressionType expressionType, SqlExpression left, SqlExpression right) {
			if (!expressionType.IsBinary())
				throw new ArgumentException();

			return new SqlBinaryExpression(expressionType, left, right);
		}

		public static SqlBinaryExpression Add(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.Add, left, right);

		public static SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.Subtract, left, right);

		#endregion
	}
}