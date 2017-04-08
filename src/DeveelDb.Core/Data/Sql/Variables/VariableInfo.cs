using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Variables {
	public sealed class VariableInfo : IDbObjectInfo {
		public VariableInfo(string name, SqlType type, bool constant, SqlExpression defaultValue) {
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (!Variable.IsValidName(name))
				throw new ArgumentException($"The variable name '{name}' is invalid");

			if (constant && defaultValue == null)
				throw new ArgumentNullException(nameof(defaultValue), "A constant variable must define a default value");

			Name = name;
			Type = type;
			Constant = constant;
			DefaultValue = defaultValue;
		}

		public string Name { get; }

		public SqlType Type { get; }

		public bool Constant { get; }

		public SqlExpression DefaultValue { get; }

		public bool HasDefaultValue => DefaultValue != null;

		DbObjectType IDbObjectInfo.ObjectType => DbObjectType.Variable;

		ObjectName IDbObjectInfo.FullName => new ObjectName(Name);
	}
}