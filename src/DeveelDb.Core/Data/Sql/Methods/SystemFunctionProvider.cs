using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class SystemFunctionProvider : SqlMethodRegistry {
		#region Utilities

		private void Register(string name, SqlMethodParameterInfo[] parameters, SqlType returnType, Func<MethodContext, Task> body) {
			var functionInfo = new SqlFunctionInfo(ObjectName.Parse(name), returnType);
			foreach (var parameter in parameters) {
				functionInfo.Parameters.Add(parameter);
			}

			var function = new SqlFunctionDelegate(functionInfo, body);
			Register(function);
		}

		private void Register(string name, SqlMethodParameterInfo param1, SqlMethodParameterInfo param2, SqlMethodParameterInfo param3, SqlType returnType,
			Func<MethodContext, Task> body) {
			Register(name, new []{param1, param2, param3}, returnType, body);
		}

		private void Register(string name, SqlMethodParameterInfo param1, SqlMethodParameterInfo param2, SqlType returnType,
			Func<MethodContext, Task> body) {
			Register(name, new[] { param1, param2}, returnType, body);
		}

		private void Register(string name, SqlMethodParameterInfo param, SqlType returnType,
			Func<MethodContext, Task> body) {
			Register(name, new[] { param }, returnType, body);
		}

		private SqlMethodParameterInfo Param(string name, SqlType type,
			SqlParameterDirection direction = SqlParameterDirection.In, SqlExpression defaultValue = null) {
			return new SqlMethodParameterInfo(name, type, defaultValue, direction);
		}

		private SqlMethodParameterInfo Deterministic(string name) {
			return Param(name, new SqlDeterministicType());
		}

		#endregion

		protected override void Initialize() {
			Register("add",
				Deterministic("a"),
				Deterministic("b"),
				new SqlDeterministicType(),
				context => {
					var a = context.Value("a");
					var b = context.Value("b");
					context.SetResult(a.Add(b));
					return Task.CompletedTask;
				});
		}
	}
}