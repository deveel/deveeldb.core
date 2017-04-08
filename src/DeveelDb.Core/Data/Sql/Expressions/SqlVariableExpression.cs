using System;

using Deveel.Data.Services;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlVariableExpression : SqlExpression {
		internal SqlVariableExpression(string variableName)
			: base(SqlExpressionType.Variable) {
			if (String.IsNullOrWhiteSpace(variableName))
				throw new ArgumentNullException(nameof(variableName));
			if (!Variables.Variable.IsValidName(variableName))
				throw new ArgumentException($"The variable name '{variableName}' is invalid.");

			VariableName = variableName;
		}

		public string VariableName { get; }

		public override bool CanReduce => true;

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.AppendFormat(":{0}", VariableName);
		}

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitVariable(this);
		}

		public override SqlExpression Reduce(IContext context) {
			if (context == null)
				throw new SqlExpressionException("A context is required to reduce a variable expression");

			// TODO: find out the configuration in context and see if it is ignoreCase
			var variable = context.ResolveVariable(VariableName, true);

			if (variable == null)
				return Constant(SqlObject.Unknown);

			return variable.Evaluate(context);
		}
	}
}