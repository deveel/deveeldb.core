using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Methods {
	public sealed class InvokeInfo {
		private readonly Dictionary<string, SqlType> arguments;

		internal InvokeInfo(SqlMethodInfo methodInfo, Dictionary<string, SqlType> arguments) {
			MethodInfo = methodInfo;
			this.arguments = arguments;
		}

		public SqlMethodInfo MethodInfo { get; }

		public IEnumerable<string> ArgumentNames => arguments.Keys;

		public bool HasArgument(string parameterName) {
			return arguments.ContainsKey(parameterName);
		}

		public SqlType ArgumentType(string parameterName) {
			SqlType type;
			if (!arguments.TryGetValue(parameterName, out type))
				return null;

			return type;
		}
	}
}