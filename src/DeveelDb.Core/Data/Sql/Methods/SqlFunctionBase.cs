using System;

namespace Deveel.Data.Sql.Methods {
	public abstract class SqlFunctionBase : SqlMethod {
		protected SqlFunctionBase(SqlFunctionInfo functionInfo)
			: base(functionInfo) {
		}

		public new SqlFunctionInfo MethodInfo => (SqlFunctionInfo) base.MethodInfo;

		public abstract FunctionType FunctionType { get; }

		public override MethodType Type => MethodType.Function;
	}
}