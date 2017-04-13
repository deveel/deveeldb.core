using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Methods {
	public class SqlProcedureDelegate : SqlProcedureBase {
		private readonly Func<MethodContext, Task> body;

		public SqlProcedureDelegate(SqlMethodInfo methodInfo, Func<MethodContext, Task> body)
			: base(methodInfo) {
			this.body = body;
		}

		public SqlProcedureDelegate(SqlMethodInfo methodInfo, Action<MethodContext> body)
			: this(methodInfo, context => {
				body(context);
				return Task.CompletedTask;
			}) {
		}

		protected override Task ExecuteContextAsync(MethodContext context) {
			return body(context);
		}
	}
}