using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlMethodInfo : IDbObjectInfo {
		public SqlMethodInfo(ObjectName fullName, MethodType type) {
			if (fullName == null)
				throw new ArgumentNullException(nameof(fullName));

			FullName = fullName;
			Type = type;
			Parameters = new ParameterCollection(this);
		}

		public MethodType Type { get; }

		public bool IsFunction => Type == MethodType.Function;

		public bool IsProcedure => Type == MethodType.Procedure;

		DbObjectType IDbObjectInfo.ObjectType => DbObjectType.Method;

		public ObjectName FullName { get; }

		public ICollection<SqlMethodParameterInfo> Parameters { get; }

		public SqlType ReturnType { get; private set; }

		public SqlMethodBody Body { get; set; }

		public void SetReturnType(SqlType type) {
			if (IsProcedure)
				throw new InvalidOperationException($"Cannot set the return type for procedure {FullName}.");

			ReturnType = type;
		}

		internal void AppendTo(SqlStringBuilder builder) {
			builder.Append(Type.ToString().ToUpperInvariant());
			builder.Append(" ");
			FullName.AppendTo(builder);

			if (IsFunction &&
			    ReturnType != null) {
				builder.Append(" RETURNS ");
				ReturnType.AppendTo(builder);
			}

			if (Body != null && Body is ISqlFormattable) {
				builder.AppendLine(" IS");
				builder.Indent();
				(Body as ISqlFormattable).AppendTo(builder);
			}
		}

		#region ParameterCollection

		class ParameterCollection : Collection<SqlMethodParameterInfo> {
			private readonly SqlMethodInfo methodInfo;

			public ParameterCollection(SqlMethodInfo methodInfo) {
				this.methodInfo = methodInfo;
			}

			private void AssertNotContains(string name) {
				if (Items.Any(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
					throw new ArgumentException($"A parameter named {name} was already specified in method '{methodInfo.FullName}'.");
			}

			private void AssetNotOutputInFunction(SqlMethodParameterInfo parameter) {
				if (parameter.IsOutput && methodInfo.IsFunction)
					throw new ArgumentException($"Trying to add the OUT parameter {parameter.Name} to the function {methodInfo.FullName}");
			}

			protected override void InsertItem(int index, SqlMethodParameterInfo item) {
				AssertNotContains(item.Name);
				AssetNotOutputInFunction(item);
				item.Offset = index;
				base.InsertItem(index, item);
			}

			protected override void SetItem(int index, SqlMethodParameterInfo item) {
				AssetNotOutputInFunction(item);
				item.Offset = index;
				base.SetItem(index, item);
			}
		}

		#endregion
	}
}