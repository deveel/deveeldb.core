using System;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlProcedureInfo : SqlMethodInfo {
		public SqlProcedureInfo(ObjectName methodName)
			: base(methodName, MethodType.Procedure) {
		}
	}
}