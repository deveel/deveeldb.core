using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlProcedure : SqlMethod {
		public SqlProcedure(SqlMethodInfo methodInfo) 
			: base(methodInfo) {
		}

		public void SetBody(Func<MethodContext, Task> body) {
			Body = SqlMethodDelegate.Procedure(MethodInfo, body);
		}

		public void SetBody(Action<MethodContext> body) {
			Body = SqlMethodDelegate.Procedure(MethodInfo, body);
		}
	}
}