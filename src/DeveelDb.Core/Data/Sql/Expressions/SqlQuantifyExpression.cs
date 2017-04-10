using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlQuantifyExpression : SqlExpression {
		internal SqlQuantifyExpression(SqlExpressionType expressionType, SqlBinaryExpression expression)
			: base(expressionType) {
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			if (!expression.ExpressionType.IsRelational())
				throw new ArgumentException("Cannot quantify a non-relational expression");

			Expression = expression;
		}

		public SqlBinaryExpression Expression { get; }

		public override bool CanReduce => true;

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitQuantify(this);
		}

		public override SqlType GetSqlType(IContext context) {
			return PrimitiveTypes.Boolean();
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			Expression.Left.AppendTo(builder);

			builder.Append(" ");
			builder.Append(Expression.GetOperatorString());
			builder.AppendFormat(" {0}", ExpressionType.ToString().ToUpperInvariant());

			if (Expression.Right is SqlQueryExpression)
				builder.Append("(");

			Expression.Right.AppendTo(builder);

			if (Expression.Right is SqlQueryExpression)
				builder.Append(")");
		}

		public override SqlExpression Reduce(IContext context) {
			if (Expression.Right is SqlQueryExpression)
				return ReduceQuery(context);

			var resultType = Expression.Right.GetSqlType(context);
			if (resultType is SqlArrayType) {
				return ReduceArray(context);
			}

			throw new NotSupportedException();
		}

		private SqlExpression ReduceQuery(IContext context) {
			throw new NotImplementedException();
		}

		private SqlExpression ReduceArray(IContext context) {
			var rightResult = Expression.Right.Reduce(context);
			if (!(rightResult is SqlConstantExpression))
				throw new InvalidOperationException();

			var rightValue = ((SqlConstantExpression) rightResult).Value;
			if (rightValue.IsNull)
				return Constant(SqlObject.Unknown);

			if (!(rightValue.Type is SqlArrayType))
				throw new InvalidOperationException("Invalid value for a quantification");

			var leftResult = Expression.Left.Reduce(context);
			if (!(leftResult is SqlConstantExpression))
				throw new NotSupportedException();

			var leftValue = ((SqlConstantExpression) leftResult).Value;
			var array = ((SqlArray) rightValue.Value);

			switch (ExpressionType) {
				case SqlExpressionType.Any:
					return IsArrayAny(Expression.ExpressionType, leftValue, array, context);
				case SqlExpressionType.All:
					return IsArrayAll(Expression.ExpressionType, leftValue, array, context);
				default:
					throw new NotSupportedException();
			}
		}

		private SqlObject Relational(SqlExpressionType opType, SqlObject a, SqlObject b) {
			switch (opType) {
				case SqlExpressionType.Equal:
					return a.Equal(b);
				case SqlExpressionType.NotEqual:
					return a.NotEqual(b);
				case SqlExpressionType.GreaterThan:
					return a.GreaterThan(b);
				case SqlExpressionType.LessThan:
					return a.LessThan(b);
				case SqlExpressionType.GreaterThanOrEqual:
					return a.GreaterThanOrEqual(b);
				case SqlExpressionType.LessThanOrEqual:
					return a.LessOrEqualThan(b);
				case SqlExpressionType.Is:
					return a.Is(b);
				case SqlExpressionType.IsNot:
					return a.IsNot(b);
				default:
					return SqlObject.Unknown;
			}
		}

		private static SqlObject ItemValue(SqlExpression item, IContext context) {
			var value = item.Reduce(context);
			if (!(value is SqlConstantExpression))
				return SqlObject.Unknown;

			return ((SqlConstantExpression) value).Value;
		}

		private SqlExpression IsArrayAll(SqlExpressionType opType, SqlObject value, SqlArray array, IContext context) {
			foreach (var item in array) {
				var itemValue = ItemValue(item, context);
				var result = Relational(opType, value, itemValue);
				if (result.IsUnknown)
					return Constant(SqlObject.Unknown);

				if (result.IsFalse)
					return Constant(SqlObject.Boolean(false));
			}

			return Constant(SqlObject.Boolean(true));
		}

		private SqlExpression IsArrayAny(SqlExpressionType opType, SqlObject value, SqlArray array, IContext context) {
			foreach (var item in array) {
				var itemValue = ItemValue(item, context);
				var result = Relational(opType, value, itemValue);
				if (result.IsUnknown)
					return Constant(SqlObject.Unknown);

				if (result.IsTrue)
					return Constant(SqlObject.Boolean(true));
			}

			return Constant(SqlObject.Boolean(false));
		}
	}
}