// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Deveel.Data.Configuration;

namespace Deveel.Data.Sql.Methods {
	public class SqlMethodInfo : IDbObjectInfo, ISqlFormattable {
		public SqlMethodInfo(ObjectName methodName) {
			if (methodName == null)
				throw new ArgumentNullException(nameof(methodName));

			MethodName = methodName;
			Parameters = new ParameterCollection(this);
		}

		DbObjectType IDbObjectInfo.ObjectType => DbObjectType.Method;

		public ObjectName MethodName { get; }

		ObjectName IDbObjectInfo.FullName => MethodName;

		public IList<SqlMethodParameterInfo> Parameters { get; }

		internal bool TryGetParameter(string name, bool ignoreCase, out SqlMethodParameterInfo paramInfo) {
			var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
			var dictionary = Parameters.ToDictionary(x => x.Name, y => y, comparer);
			return dictionary.TryGetValue(name, out paramInfo);
		}

		internal virtual void AppendTo(SqlStringBuilder builder) {
			MethodName.AppendTo(builder);

			AppendParametersTo(builder);
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		internal void AppendParametersTo(SqlStringBuilder builder) {
			builder.Append("(");

			if (Parameters != null) {
				for (int i = 0; i < Parameters.Count; i++) {
					Parameters[i].AppendTo(builder);

					if (i < Parameters.Count - 1)
						builder.Append(", ");
				}
			}

			builder.Append(")");
		}

		public override string ToString() {
			return this.ToSqlString();
		}

		public bool Matches(IContext context, Invoke invoke) {
			return Matches(this, context, invoke);
		}

		public static bool Matches(SqlMethodInfo methodInfo, IContext context, Invoke invoke) {
			var ignoreCase = context.GetValue("ignoreCase", true);

			if (!methodInfo.MethodName.Equals(invoke.MethodName, ignoreCase))
				return false;
			if (methodInfo.Parameters.Count != invoke.Arguments.Count)
				return false;

			for (int i = 0; i < invoke.Arguments.Count; i++) {
				var arg = invoke.Arguments[i];

				SqlMethodParameterInfo paramInfo;
				if (arg.IsNamed) {
					if (!methodInfo.TryGetParameter(arg.ParameterName, ignoreCase, out paramInfo))
						return false;
				} else {
					paramInfo = methodInfo.Parameters[i];
				}

				var argType = arg.Value.GetSqlType(context);
				if (!argType.IsComparable(paramInfo.ParameterType))
					return false;
			}

			return true;

		}

		#region ParameterCollection

		class ParameterCollection : Collection<SqlMethodParameterInfo> {
			private readonly SqlMethodInfo methodInfo;

			public ParameterCollection(SqlMethodInfo methodInfo) {
				this.methodInfo = methodInfo;
			}

			private void AssertNotContains(string name) {
				if (Items.Any(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
					throw new ArgumentException($"A parameter named {name} was already specified in method '{methodInfo.MethodName}'.");
			}

			protected override void InsertItem(int index, SqlMethodParameterInfo item) {
				AssertNotContains(item.Name);
				item.Offset = index;
				base.InsertItem(index, item);
			}

			protected override void SetItem(int index, SqlMethodParameterInfo item) {
				item.Offset = index;
				base.SetItem(index, item);
			}
		}

		#endregion
	}
}