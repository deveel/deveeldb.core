using System;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlMethodParameterInfo {
		public SqlMethodParameterInfo(string name, SqlType parameterType, SqlParameterDirection direction) {
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
	}
}