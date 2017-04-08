using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlMethodResult {
		private readonly Dictionary<string, SqlExpression> output;

		public SqlMethodResult(SqlMethodInfo methodInfo) {
			MethodInfo = methodInfo;
			ResultValue = SqlExpression.Constant(SqlObject.Null);
			output = new Dictionary<string, SqlExpression>();
		}

		public SqlMethodInfo MethodInfo { get; }

		public SqlExpression ResultValue { get; private set; }

		public bool HasResult { get; private set; }

		public IDictionary<string, SqlExpression> Output => new ReadOnlyDictionary<string, SqlExpression>(output);

		public void SetOutput(string parameterName, SqlExpression value) {
			if (String.IsNullOrWhiteSpace(parameterName))
				throw new ArgumentNullException(nameof(parameterName));

			if (!MethodInfo.IsProcedure)
				throw new InvalidOperationException($"The method {MethodInfo.FullName} is not a Procedure");

			SqlMethodParameterInfo parameter;
			if (!MethodInfo.Parameters.ToDictionary(x => x.Name, y => y).TryGetValue(parameterName, out parameter))
				throw new ArgumentException($"The method {MethodInfo.FullName} contains no parameter {parameterName}");

			if (!parameter.IsOutput)
				throw new ArgumentException($"The parameter {parameter.Name} is not an OUTPUT parameter");

			output[parameterName] = value;
		}

		public void SetResult(SqlExpression value, IContext context) {
			if (!MethodInfo.IsFunction)
				throw new InvalidOperationException($"Trying to set the return type to the method {MethodInfo.FullName} that is not a function.");

			var valueType = value.ReturnType(context);
			if (!valueType.IsComparable(MethodInfo.ReturnType))
				throw new InvalidOperationException($"The result type {valueType} of the expression is not compatible " +
				                                    $"with the return type {MethodInfo.ReturnType} of the function {MethodInfo.FullName}");

			ResultValue = value;
			HasResult = true;
		}
	}
}