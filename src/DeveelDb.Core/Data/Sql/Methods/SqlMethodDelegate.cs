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

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("[delegate]");
		}

		private static Func<MethodContext, Task> MakeFunction(Action<MethodContext> action) {
			return context => {
				action(context);
				return Task.CompletedTask;
			};
		}

		public static SqlMethodDelegate Function(SqlFunctionInfo functionInfo, Func<MethodContext, Task<SqlExpression>> function) {
			Func<MethodContext, Task> body = async context => {
				var result = await function(context);
				context.SetResult(result);
			};

			return new SqlMethodDelegate(functionInfo, MethodType.Function, body);
		}

		public static SqlMethodDelegate Function(SqlFunctionInfo functionInfo, Func<MethodContext, Task<SqlObject>> function) {
			Func<MethodContext, Task<SqlExpression>> body = async context => {
				var result = await function(context);
				return SqlExpression.Constant(result);
			};

			return Function(functionInfo, body);
		}
	}
}