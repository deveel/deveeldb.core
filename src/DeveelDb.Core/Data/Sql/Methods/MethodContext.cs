using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Methods {
	public sealed class MethodContext : Context {
		private Dictionary<string, SqlExpression> output;
		private List<SqlExpression> args;
		private Dictionary<string, int> argsMap;

		internal MethodContext(IContext context, SqlMethodInfo methodInfo, Invoke invoke)
			: base(context) {
			Invoke = invoke;
			MethodInfo = methodInfo;

			OrderArguments();

			ResultValue = SqlExpression.Constant(SqlObject.Null);
			output = new Dictionary<string, SqlExpression>();

			ContextScope.RegisterInstance<IVariableResolver>(invoke);
		}

		protected override string ContextName => MethodInfo.MethodName.FullName;

		public SqlMethodInfo MethodInfo { get; }

		public Invoke Invoke { get; private set; }

		internal SqlExpression ResultValue { get; private set; }

		internal bool HasResult { get; private set; }

		public SqlExpression GetArgument(int offset) {
			return args[offset];
		}

		private void OrderArguments() {
			throw new NotImplementedException();
		}

		internal SqlMethodResult CreateResult() {
			return new SqlMethodResult(ResultValue, HasResult, output);
		}

		public void SetOutput(string parameterName, SqlExpression value) {
			if (String.IsNullOrWhiteSpace(parameterName))
				throw new ArgumentNullException(nameof(parameterName));

			if (!MethodInfo.IsProcedure)
				throw new InvalidOperationException($"The method {MethodInfo.MethodName} is not a Procedure");

			SqlMethodParameterInfo parameter;
			if (!MethodInfo.Parameters.ToDictionary(x => x.Name, y => y).TryGetValue(parameterName, out parameter))
				throw new ArgumentException($"The method {MethodInfo.MethodName} contains no parameter {parameterName}");

			if (!parameter.IsOutput)
				throw new ArgumentException($"The parameter {parameter.Name} is not an OUTPUT parameter");

			output[parameterName] = value;
		}

		public void SetResult(SqlExpression value, IContext context) {
			if (!MethodInfo.IsFunction)
				throw new InvalidOperationException($"Trying to set the return type to the method {MethodInfo.MethodName} that is not a function.");

			var functionInfo = (SqlFunctionInfo) MethodInfo;
			var valueType = value.ReturnType(context);
			if (!valueType.IsComparable(functionInfo.ReturnType))
				throw new InvalidOperationException($"The result type {valueType} of the expression is not compatible " +
				                                    $"with the return type {functionInfo.ReturnType} of the function {MethodInfo.MethodName}");

			ResultValue = value;
			HasResult = true;
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (output != null)
					output.Clear();
			}

			output = null;
			ResultValue = null;
			base.Dispose(disposing);
		}
	}
}