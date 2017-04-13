using System;

namespace Deveel.Data.Sql.Methods {
	public abstract class SqlProcedureBase : SqlMethod {
		protected SqlProcedureBase(SqlMethodInfo methodInfo)
			: base(methodInfo) {
		}

		public override MethodType Type => MethodType.Procedure;
	}
}