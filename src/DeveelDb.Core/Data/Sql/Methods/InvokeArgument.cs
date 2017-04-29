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

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class InvokeArgument : ISqlFormattable, ISerializable {
		public InvokeArgument(SqlExpression value) 
			: this(null, value) {
		}

		public InvokeArgument(string parameterName, SqlExpression value) {
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			ParameterName = parameterName;
			Value = value;
		}

		public InvokeArgument(SqlObject value)
			: this(null, value) {
		}

		public InvokeArgument(string parameterName, SqlObject value)
			: this(parameterName, SqlExpression.Constant(value)) {
		}

		private InvokeArgument(SerializationInfo info) {
			ParameterName = info.GetString("name");
			Value = info.GetValue<SqlExpression>("value");
		}

		public string ParameterName { get; }

		public bool IsNamed => !String.IsNullOrEmpty(ParameterName);

		public SqlExpression Value { get; }

		public override string ToString() {
			return this.ToSqlString();
		}

		void ISerializable.GetObjectData(SerializationInfo info) {
			info.SetValue("name", ParameterName);
			info.SetValue("value", Value);
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			if (IsNamed)
				builder.AppendFormat("{0} => ", ParameterName);

			Value.AppendTo(builder);
		}
	}
}