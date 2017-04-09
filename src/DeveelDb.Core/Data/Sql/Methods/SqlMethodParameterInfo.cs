using System;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlMethodParameterInfo : ISqlFormattable {
		public SqlMethodParameterInfo(string name, SqlType parameterType) 
			: this(name, parameterType, SqlParameterDirection.In) {
		}

		public SqlMethodParameterInfo(string name, SqlType parameterType, SqlParameterDirection direction) {
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));
			if (parameterType == null)
				throw new ArgumentNullException(nameof(parameterType));

			Name = name;
			ParameterType = parameterType;
			Direction = direction;
		}

		public SqlType ParameterType { get; }

		public string Name { get; }

		public SqlParameterDirection Direction { get; }

		public int Offset { get; internal set; }

		public bool IsOutput => (Direction & SqlParameterDirection.Out) != 0;

		public bool IsInput => (Direction & SqlParameterDirection.In) != 0;

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			builder.Append(Name);
			builder.Append(" ");
			ParameterType.AppendTo(builder);

			if (Direction != SqlParameterDirection.In) {
				builder.Append(" ");

				if (IsInput)
					builder.Append("INPUT");
				if (IsInput && IsOutput)
					builder.Append(" ");
				if (IsOutput)
					builder.Append("OUTPUT");
			}
		}

		public override string ToString() {
			return this.ToSqlString();
		}
	}
}