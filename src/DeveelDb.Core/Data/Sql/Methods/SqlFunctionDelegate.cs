using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlFunctionDelegate : SqlFunctionBase {
		private readonly Func<MethodContext, Task> body;

		public SqlFunctionDelegate(SqlFunctionInfo functionInfo, Func<MethodContext, Task> body)
			: base(functionInfo) {
			this.body = body;
		}

		public SqlFunctionDelegate(SqlFunctionInfo functionInfo, Func<MethodContext, Task<SqlObject>> body)
			: this(functionInfo, async context => {
				var result = await body(context);
				context.SetResult(result);
			}) {
		}

		public SqlFunctionDelegate(SqlFunctionInfo functionInfo, Func<MethodContext, SqlExpression> body)
			: this(functionInfo, context => {
				var result = body(context);
				context.SetResult(result);
				return Task.CompletedTask;
			}) {
		}

		public override FunctionType FunctionType => FunctionType.Scalar;

		protected override Task ExecuteContextAsync(MethodContext context) {
			return body(context);
		}
	}
}