using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlConditionExpression : SqlExpression {
		internal SqlConditionExpression(SqlExpression test, SqlExpression ifTrue, SqlExpression ifFalse)
			: base(SqlExpressionType.Condition) {
			if (test == null)
				throw new ArgumentNullException(nameof(test));
			if (ifTrue == null)
				throw new ArgumentNullException(nameof(ifTrue));
			if (ifFalse == null)
				throw new ArgumentNullException(nameof(ifFalse));

			Test = test;
			IfTrue = ifTrue;
			IfFalse = ifFalse;
		}

		public SqlExpression Test { get; }

		public SqlExpression IfTrue { get; }

		public SqlExpression IfFalse { get; }

		public override bool CanReduce => true;

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitCondition(this);
		}

		public override SqlExpression Reduce(IContext context) {
			var returnType = Test.ReturnType(context);
			if (!(returnType is SqlBooleanType))
				throw new InvalidOperationException("The expression test has not a BOOLEAN result");

			var testResult = Test.Reduce(context);
			if (testResult.ExpressionType != SqlExpressionType.Constant)
				throw new InvalidOperationException();

			var value = ((SqlConstantExpression) testResult).Value;
			if (value.IsNull || value.IsUnknown)
				return Constant(value);

			if (value.IsTrue)
				return IfTrue.Reduce(context);
			if (value.IsFalse)
				return IfFalse.Reduce(context);

			return base.Reduce(context);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("CASE WHEN ");
			Test.AppendTo(builder);
			builder.Append(" THEN ");
			IfTrue.AppendTo(builder);
			builder.Append(" ELSE ");
			IfFalse.AppendTo(builder);
			builder.Append(" END");
		}
	}
}