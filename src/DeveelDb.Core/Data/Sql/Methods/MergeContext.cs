using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class MergeContext : Context {
		internal MergeContext(MethodContext parent, SqlObject accumulated)
			: base(parent, $"Aggregate({parent.Method.MethodInfo.MethodName})") {
			Accumulated = accumulated;
		}

		public SqlObject Accumulated { get; }

		internal SqlObject Output { get; private set; }

		public void SetOutput(SqlObject output) {
			if (output.IsNull)
				throw new NotSupportedException();

			Output = output;
		}
	}
}