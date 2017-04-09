using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Methods {
	public abstract class SqlMethodBody : ISqlFormattable {
		protected SqlMethodBody(SqlMethodInfo methodInfo, MethodType methodType) {
			MethodInfo = methodInfo;
			MethodType = methodType;
		}

		public MethodType MethodType { get; }

		public SqlMethodInfo MethodInfo { get; }

		public abstract Task ExecuteAsync(MethodContext context);

		protected virtual void AppendTo(SqlStringBuilder builder) {
			
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}
	}
}