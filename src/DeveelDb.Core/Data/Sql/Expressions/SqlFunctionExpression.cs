// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.Threading.Tasks;

using Deveel.Data.Security;
using Deveel.Data.Services;
using Deveel.Data.Sql.Methods;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlFunctionExpression : SqlExpression {
		public ObjectName FunctionName { get; }

		public InvokeArgument[] Arguments { get; }

		internal SqlFunctionExpression(ObjectName functionName, InvokeArgument[] arguments)
			: base(SqlExpressionType.Function) {
			FunctionName = functionName;
			Arguments = arguments;
		}

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitFunction(this);
		}

		public override bool IsReference => true;

		public override bool CanReduce => true;

		protected override void AppendTo(SqlStringBuilder builder) {
			FunctionName.AppendTo(builder);
			builder.Append("(");

			for (int i = 0; i < Arguments.Length; i++) {
				Arguments[i].AppendTo(builder);

				if (i < Arguments.Length - 1)
					builder.Append(", ");
			}

			builder.Append(")");
		}

		private SqlFunctionBase ResolveFunction(IContext context) {
			if (context == null)
				throw new SqlExpressionException();

			var resolver = context.Scope.Resolve<IMethodResolver>();
			if (resolver == null)
				throw new SqlExpressionException();

			var method = resolver.ResolveMethod(context, new Invoke(FunctionName, Arguments));
			if (method == null)
				throw new SqlExpressionException();

			if (!method.IsFunction)
				throw new SqlExpressionException();

			return ((SqlFunctionBase) method);
		}

		public override SqlType GetSqlType(IContext context) {
			var function = ResolveFunction(context);
			var functionInfo = function.MethodInfo;
			return functionInfo.ReturnType;
		}

		public override async Task<SqlExpression> ReduceAsync(IContext context) {
			var function = ResolveFunction(context);

			if (!function.IsSystem && !context.UserCanExecute(FunctionName))
				throw new UnauthorizedAccessException();

			var result = await function.ExecuteAsync(context, Arguments);
			if (!result.HasReturnedValue)
				throw new SqlExpressionException();

			return result.ReturnedValue;
		}
	}
}