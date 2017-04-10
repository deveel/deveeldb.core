using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlFunctionInfo : SqlMethodInfo {
		public SqlFunctionInfo(ObjectName functionName, SqlType returnType)
			: this(functionName, FunctionType.Scalar, returnType) {
		}

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

		public void SetFunctionBody(Func<MethodContext, Task<SqlObject>> body) {
			if (!IsFunction)
				throw new InvalidOperationException($"Trying to set a function body to {MethodName} that is not a function");

			Body = SqlMethodDelegate.Function(this, body);
		}

		public void SetFunctionBody(Func<MethodContext, Task<SqlExpression>> body) {
			if (!IsFunction)
				throw new InvalidOperationException($"Trying to set a function body to {MethodName} that is not a function");

			Body = SqlMethodDelegate.Function(this, body);

		}
	}
}