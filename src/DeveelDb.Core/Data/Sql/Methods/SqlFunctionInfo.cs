using System;
using System.Reflection;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlFunctionInfo : SqlMethodInfo {
		public SqlFunctionInfo(ObjectName functionName, FunctionType functionType, SqlType returnType)
			: base(functionName, MethodType.Function) {
			if (returnType == null)
				throw new ArgumentNullException(nameof(returnType));

			FunctionType = functionType;
			ReturnType = returnType;
		}

		public FunctionType FunctionType { get; }

		public SqlType ReturnType { get; }

		internal override void AppendTo(SqlStringBuilder builder) {
			builder.Append(Type.ToString().ToUpperInvariant());
			builder.Append(" ");
			MethodName.AppendTo(builder);

			AppendParametersTo(builder);

			builder.Append(" RETURNS ");
			ReturnType.AppendTo(builder);

			if (Body != null) {
				builder.AppendLine(" IS");
				builder.Indent();
				Body.AppendTo(builder);
			}
		}
	}
}