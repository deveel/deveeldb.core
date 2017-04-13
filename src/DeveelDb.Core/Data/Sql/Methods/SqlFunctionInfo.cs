using System;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlFunctionInfo : SqlMethodInfo {
		public SqlFunctionInfo(ObjectName functionName, SqlType returnType)
			: base(functionName) {
			if (returnType == null)
				throw new ArgumentNullException(nameof(returnType));

			ReturnType = returnType;
		}

		public SqlType ReturnType { get; }

		internal override void AppendTo(SqlStringBuilder builder) {
			MethodName.AppendTo(builder);

			AppendParametersTo(builder);

			builder.Append(" RETURNS ");
			ReturnType.AppendTo(builder);
		}
	}
}