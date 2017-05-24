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
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Statements {
	public sealed class AssignStatement : SqlStatement, IPlSqlStatement {
		public AssignStatement(string variable, SqlExpression value) {
			if (String.IsNullOrWhiteSpace(variable))
				throw new ArgumentNullException(nameof(variable));
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			Variable = variable;
			Value = value;
		}

		private AssignStatement(SerializationInfo info)
			: base(info) {
			Variable = info.GetString("var");
			Value = info.GetValue<SqlExpression>("value");
		}

		public string Variable { get; }

		public SqlExpression Value { get; }

		protected override SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			var value = Value.Prepare(preparer);
			return new AssignStatement(Variable, value);
		}

		protected override Task ExecuteStatementAsync(StatementContext context) {
			try {
				var value = context.AssignVariable(Variable, Value);
				context.SetResult(value);
			} catch (SqlExpressionException ex) {
				throw new SqlStatementException($"Could not assign the variable '{Variable}' because of an error", ex);
			} catch (Exception ex) {
				throw new SqlStatementException($"Could not assign the variable '{Variable}' because of an error", ex);
			}

			return Task.CompletedTask;
		}

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("var", Variable);
			info.SetValue("value", Value);

			base.GetObjectData(info);
		}

		protected override void GetMetadata(IDictionary<string, object> data) {
			data["var"] = Variable;
			data["value"] = Value.ToString();
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append(Variable);
			builder.Append(" := ");
			Value.AppendTo(builder);
			builder.Append(";");
		}
	}
}