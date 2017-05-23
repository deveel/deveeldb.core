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
	public sealed class ConditionalStatement : CodeBlockStatement, IPlSqlStatement {
		public ConditionalStatement(SqlExpression condition)
			: this(condition, (SqlStatement) null) {
		}

		public ConditionalStatement(SqlExpression condition, SqlStatement @else)
			: this(condition, null, @else) {
		}

		public ConditionalStatement(SqlExpression condition, string label)
			: this(condition, label, null) {
		}

		public ConditionalStatement(SqlExpression condition, string label, SqlStatement @else)
			: base(label) {
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));

			Condition = condition;
			Else = @else;
		}

		public ConditionalStatement(SerializationInfo info)
			: base(info) {
			Condition = info.GetValue<SqlExpression>("test");
			Else = info.GetValue<SqlStatement>("else");
		}

		public SqlExpression Condition { get; }

		public SqlStatement Else { get; }

		public override bool CanPrepare => Else != null;

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("test", Condition);
			info.SetValue("else", Else);
			base.GetObjectData(info);
		}

		protected override SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			var test = Condition.Prepare(preparer);
			return new ConditionalStatement(test, Label, Else);
		}

		protected override SqlStatement PrepareStatement(IContext context) {
			var @else = Else;
			if (@else != null)
				@else = @else.Prepare(context);

			return new ConditionalStatement(Condition, Label, @else);
		}

		protected override async Task ExecuteStatementAsync(StatementContext context) {
			var testResult = await Condition.ReduceToConstantAsync(context);
			if (testResult.IsTrue) {
				foreach (var child in Statements) {
					var childContext = new StatementContext(context, child);
					await child.ExecuteAsync(childContext);
				}
			} else if (testResult.IsFalse && Else != null) {
				await Else.ExecuteAsync(context);
			} else if (testResult.IsNull || testResult.IsUnknown) {
				throw new SqlStatementException("The result of the test expression is invalid");
			}
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			if (!String.IsNullOrWhiteSpace(Label))
				builder.AppendLine($"<<{Label}>>");

			builder.Append("IF ");
			Condition.AppendTo(builder);
			builder.AppendLine(" THEN");

			builder.Indent();

			foreach (var statement in Statements) {
				statement.AppendTo(builder);
				builder.AppendLine();
			}

			builder.DeIndent();

			if (Else != null) {
				builder.AppendLine("ELSE");

				builder.Indent();
				Else.AppendTo(builder);
				builder.DeIndent();

				builder.AppendLine();
			}

			builder.Append("END IF;");
		}
	}
}