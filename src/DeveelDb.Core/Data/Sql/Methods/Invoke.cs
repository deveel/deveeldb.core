using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Methods {
	public sealed class Invoke : ISqlFormattable {
		public Invoke(ObjectName methodName) {
			if (methodName == null)
				throw new ArgumentNullException(nameof(methodName));

			MethodName = methodName;
			Arguments = new ArgumentList(this);
		}

		public ObjectName MethodName { get; }

		public IList<InvokeArgument> Arguments { get; }

		public bool IsNamed => Arguments.Any(x => x.IsNamed);

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

		#region ArgumentList

		class ArgumentList : Collection<InvokeArgument> {
			private readonly Invoke invoke;

			public ArgumentList(Invoke invoke) {
				this.invoke = invoke;
			}

			private void ValidateArgument(InvokeArgument item) {
				if (!item.IsNamed && invoke.IsNamed)
					throw new ArgumentException("The invoke context has named items");
				if (item.IsNamed && !invoke.IsNamed && Items.Count > 0)
					throw new ArgumentException("Cannot insert a named item in an anonymous context");
			}

			protected override void SetItem(int index, InvokeArgument item) {
				ValidateArgument(item);

				if (item.IsNamed) {
					var existing = base.Items[index];
					if (existing.ParameterName != item.ParameterName)
						throw new ArgumentException();
				}

				base.SetItem(index, item);
			}

			protected override void InsertItem(int index, InvokeArgument item) {
				ValidateArgument(item);
				base.InsertItem(index, item);
			}
		}

		#endregion
	}
}