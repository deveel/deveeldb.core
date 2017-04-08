using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class InvokeArgument : ISqlFormattable {
		public InvokeArgument(SqlExpression value) 
			: this(null, value) {
		}

		public InvokeArgument(string parameterName, SqlExpression value) {
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			ParameterName = parameterName;
			Value = value;
		}

		public string ParameterName { get; }

		public bool IsNamed => !String.IsNullOrEmpty(ParameterName);

		public SqlExpression Value { get; }

		public int Offset { get; internal set; }

		public override string ToString() {
			return this.ToSqlString();
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			if (IsNamed)
				builder.AppendFormat("{0} => ", ParameterName);

			Value.AppendTo(builder);
		}
	}
}