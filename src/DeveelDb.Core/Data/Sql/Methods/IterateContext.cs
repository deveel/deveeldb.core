using System;

namespace Deveel.Data.Sql.Methods {
	public sealed class IterateContext : Context {
		internal IterateContext(MethodContext parent, SqlObject accumulation, SqlObject current)
			: base(parent, $"Aggreate({parent.Method.MethodInfo.MethodName})") {
			Accumulation = accumulation;
			Current = current;
		}

		public SqlObject Accumulation { get; }

		public SqlObject Current { get; }

		public bool IsFirst => Accumulation == null;

		internal SqlObject Result { get; private set; }

		public void SetResult(SqlObject value) {
			Result = value;
		}
	}
}