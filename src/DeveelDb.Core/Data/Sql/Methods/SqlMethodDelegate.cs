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

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlMethodDelegate : SqlMethodBody {
		private readonly Func<MethodContext, Task> func;

		public SqlMethodDelegate(SqlMethodInfo methodInfo, MethodType methodType, Func<MethodContext, Task> func) 
			: base(methodInfo, methodType) {
			this.func = func;
		}

		public SqlMethodDelegate(SqlMethodInfo methodInfo, MethodType methodType, Action<MethodContext> action)
			: this(methodInfo, methodType, MakeFunction(action)) {
		}

		public override Task ExecuteAsync(MethodContext context) {
			return func(context);
		}

		private static Func<MethodContext, Task> MakeFunction(Action<MethodContext> action) {
			return context => {
				action(context);
				return Task.CompletedTask;
			};
		}

		public static SqlMethodDelegate Function(SqlMethodInfo functionInfo, Func<MethodContext, Task<SqlExpression>> function) {
			Func<MethodContext, Task> body = async context => {
				var result = await function(context);
				context.SetResult(result);
			};

			return new SqlMethodDelegate(functionInfo, MethodType.Function, body);
		}

		public static SqlMethodDelegate Function(SqlMethodInfo functionInfo, Func<MethodContext, Task<SqlObject>> function) {
			Func<MethodContext, Task<SqlExpression>> body = async context => {
				var result = await function(context);
				return SqlExpression.Constant(result);
			};

			return Function(functionInfo, body);
		}

		public static SqlMethodDelegate Procedure(SqlMethodInfo methodInfo, Func<MethodContext, Task> procedure) {
			return new SqlMethodDelegate(methodInfo, MethodType.Procedure, procedure);
		}

		public static SqlMethodDelegate Procedure(SqlMethodInfo methodInfo, Action<MethodContext> procedure) {
			Func<MethodContext, Task> body = context => {
				procedure(context);
				return Task.CompletedTask;
			};

			return Procedure(methodInfo, body);
		}
	}
}