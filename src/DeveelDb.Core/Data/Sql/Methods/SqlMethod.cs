using System;

namespace Deveel.Data.Sql.Methods {
	public abstract class SqlMethod : ISqlFormattable {
		protected SqlMethod(SqlMethodInfo methodInfo) {
			MethodInfo = methodInfo;
		}

		public SqlMethodInfo MethodInfo { get; }

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			MethodInfo.AppendTo(builder);
		}

		public override string ToString() {
			return this.ToSqlString();
		}
	}
}