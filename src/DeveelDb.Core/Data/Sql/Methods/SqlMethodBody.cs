using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Methods {
	public abstract class SqlMethodBody {
		protected SqlMethodBody(SqlMethodInfo methodInfo, MethodType methodType) {
			MethodInfo = methodInfo;
			MethodType = methodType;
		}

		public MethodType MethodType { get; }

		public SqlMethodInfo MethodInfo { get; }

		public Task<SqlMethodResult> ExecuteAsync() {
			throw new NotImplementedException();
		}
	}
}