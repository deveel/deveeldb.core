using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Variables {
	public sealed class Variable : ISqlFormattable {
		private static readonly char[] InvalidChars = " $.|\\:/#'".ToCharArray();

		public Variable(string name, SqlType type) 
			: this(name, type, null) {
		}

		public Variable(string name, SqlType type, SqlExpression defaultValue) 
			: this(name, type, false, defaultValue) {
		}

		public Variable(string name, SqlType type, bool constant, SqlExpression defaultValue) {
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (!IsValidName(name))
				throw new ArgumentException($"The variable name '{name}' is invalid");

			if (constant && defaultValue == null)
				throw new ArgumentNullException(nameof(defaultValue), "A constant variable must define a default value");

			Name = name;
			Type = type;
			Constant = constant;
			DefaultValue = defaultValue;
		}

		public string Name { get; }

		public bool Constant { get; }

		public SqlType Type { get; }

		public SqlExpression DefaultValue { get; }

		public bool HasDefaultValue => DefaultValue != null;

		public SqlExpression Value { get; private set; }

		public SqlExpression SetValue(SqlExpression value, IContext context) {
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (Constant)
				throw new VariableException($"Cannot set constant variable {Name}");

			var valueType = value.ReturnType(context);
			if (!valueType.Equals(Type) &&
				!valueType.IsComparable(Type))
				throw new ArgumentException($"The type {valueType} of the value is not compatible with the variable type '{Type}'");

			Value = value;
			return Value;
		}

		public SqlExpression Evaluate(IContext context) {
			var expression = Value;
			if (expression == null)
				expression = DefaultValue;

			if (expression == null)
				throw new VariableException($"Variable {Name} has no value set");

			return expression.Reduce(context);
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			builder.AppendFormat(":{0}", Name);
			builder.Append(" ");
			if (Constant)
				builder.Append("CONSTANT ");

			Type.AppendTo(builder);

			if (HasDefaultValue) {
				builder.Append(" := ");
				DefaultValue.AppendTo(builder);
			}
		}

		public override string ToString() {
			return this.ToSqlString();
		}

		public static bool IsValidName(string name) {
			return name.IndexOfAny(InvalidChars) == -1;
		}
	}
}