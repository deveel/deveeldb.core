using System;

using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlVariableAssignExpression : SqlExpression {
		public string VariableName { get; }

		public SqlExpression Value { get; }

		internal SqlVariableAssignExpression(string variableName, SqlExpression value)
			: base(SqlExpressionType.VariableAssign) {
			if (String.IsNullOrWhiteSpace(variableName))
				throw new ArgumentNullException(nameof(variableName));
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (!Variables.Variable.IsValidName(variableName))
				throw new ArgumentException($"Variable name '{variableName}' is invalid.");

			VariableName = variableName;
			Value = value;
		}

		public override bool CanReduce => true;

		public override SqlExpression Reduce(IContext context) {
			if (context == null)
				throw new SqlExpressionException("A context is required to reduce a variable expression");

			var variable = context.ResolveVariable(VariableName);

			if (variable == null)
				return Constant(SqlObject.Unknown);

			return variable.SetValue(Value, context);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.AppendFormat(":{0}", VariableName);
			builder.Append(" := ");
			Value.AppendTo(builder);
		}
	}
}