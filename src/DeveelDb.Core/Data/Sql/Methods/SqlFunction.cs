using System;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public class SqlFunction : SqlMethod {
		public SqlFunction(SqlFunctionInfo methodInfo) 
			: base(methodInfo) {
		}

		public void SetBody(Func<MethodContext, Task<SqlObject>> body) {
			Body = SqlMethodDelegate.Function((SqlFunctionInfo) MethodInfo, body);
		}

		public void SetBody(Func<MethodContext, Task<SqlExpression>> body) {
			Body = SqlMethodDelegate.Function((SqlFunctionInfo) MethodInfo, body);
		}
	}
}