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
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public sealed class ReturnStatement : SqlStatement, IPlSqlStatement {
		public ReturnStatement() 
			: this((SqlExpression) null) {
		}

		public ReturnStatement(SqlExpression value) {
			Value = value;
		}

		private ReturnStatement(SerializationInfo info)
			: base(info) {
			Value = info.GetValue<SqlExpression>("value");
		}

		public SqlExpression Value { get; }

		protected override SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			var value = Value;
			if (value != null)
				value = value.Prepare(preparer);

			return new ReturnStatement(value);
		}

		protected override async Task ExecuteStatementAsync(StatementContext context) {
			var result = await Value.ReduceToConstantAsync(context);

			context.Return(result);
		}

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("value", Value);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("RETURN");

			if (Value != null) {
				builder.Append(" ");
				Value.AppendTo(builder);
			}

			builder.Append(";");
		}
	}
}