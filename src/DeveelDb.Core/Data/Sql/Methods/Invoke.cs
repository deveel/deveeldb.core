using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Methods {
	public sealed class Invoke : ISqlFormattable {
		public Invoke(ObjectName methodName) {
			if (methodName == null)
				throw new ArgumentNullException(nameof(methodName));

			MethodName = methodName;
			Arguments = new List<InvokeArgument>();
		}

		public ObjectName MethodName { get; }

		public IList<InvokeArgument> Arguments { get; }

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			MethodName.AppendTo(builder);
			builder.Append("(");

			if (Arguments.Count > 0) {
				for (int i = 0; i < Arguments.Count; i++) {
					Arguments[i].AppendTo(builder);

					if (i < Arguments.Count - 1)
						builder.Append(", ");
				}
			}

			builder.Append(")");
		}

		public override string ToString() {
			return this.ToSqlString();
		}
	}
}