using System;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public abstract class SqlMethod : ISqlFormattable {
		protected SqlMethod(SqlMethodInfo methodInfo) {
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			MethodInfo = methodInfo;
		}

		public SqlMethodInfo MethodInfo { get; }

		public async Task<SqlMethodResult> ExecuteAsync(IContext context, Invoke invoke) {
			using (var methodContext = new MethodContext(context, MethodInfo, invoke)) {
				await ExecuteContextAsync(methodContext);

				var result = methodContext.CreateResult();

				result.Validate(MethodInfo, context);

				return result;
			}
		}

		protected virtual Task ExecuteContextAsync(MethodContext context) {
			var body = MethodInfo.Body;
			if (body == null)
				throw new InvalidOperationException();

			return body.ExecuteAsync(context);
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			MethodInfo.AppendTo(builder);
		}

		public override string ToString() {
			return this.ToSqlString();
		}

		public bool MatchesInvoke(Invoke invoke, IContext context) {
			var ignoreCase = context.GetValue("ignoreCase", true);

			if (!MethodInfo.MethodName.Equals(invoke.MethodName, ignoreCase))
				return false;
			if (MethodInfo.Parameters.Count != invoke.Arguments.Count)
				return false;

			for (int i = 0; i < invoke.Arguments.Count; i++) {
				var arg = invoke.Arguments[i];

				SqlMethodParameterInfo paramInfo;
				if (arg.IsNamed) {
					if (!MethodInfo.TryGetParameter(arg.ParameterName, ignoreCase, out paramInfo))
						return false;
				} else {
					paramInfo = MethodInfo.Parameters[i];
				}

				var argType = arg.Value.ReturnType(context);
				if (!argType.IsComparable(paramInfo.ParameterType))
					return false;
			}

			return true;
		}
	}
}