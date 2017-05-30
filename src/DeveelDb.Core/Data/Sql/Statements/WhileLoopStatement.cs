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
	public sealed class WhileLoopStatement : LoopStatement {
		public WhileLoopStatement(SqlExpression condition)
			: this(condition, null) {
		}

		public WhileLoopStatement(SqlExpression condition, string label)
			: base(label) {
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));

			Condition = condition;
		}

		private WhileLoopStatement(SerializationInfo info)
			: base(info) {
			Condition = info.GetValue<SqlExpression>("condition");
		}

		public SqlExpression Condition { get; }

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("condition", Condition);

			base.GetObjectData(info);
		}

		protected override async Task<bool> CanLoopAsync(StatementContext context) {
			var result = await Condition.ReduceToConstantAsync(context);
			if (result.IsNull || result.IsUnknown)
				throw new SqlExpressionException("The condition expression reduced to an invalid state");

			return result.IsTrue;
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			if (!String.IsNullOrWhiteSpace(Label)) {
				builder.AppendFormat("<<{0}>>", Label);
				builder.AppendLine();
			}

			builder.Append("WHILE ");
			Condition.AppendTo(builder);
			builder.AppendLine();

			builder.AppendLine("LOOP");
			builder.Indent();

			foreach (var statement in Statements) {
				statement.AppendTo(builder);
				builder.AppendLine();
			}

			builder.DeIndent();
			builder.Append("END LOOP;");
		}
	}
}