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

namespace Deveel.Data.Sql.Statements {
	public class LoopStatement : CodeBlockStatement, IPlSqlStatement {
		public LoopStatement()
			: this((string) null) {
		}

		public LoopStatement(string label)
			: base(label) {
		}

		internal LoopStatement(SerializationInfo info)
			: base(info) {
		}


		internal virtual LoopStatement CreateNew() {
			return new LoopStatement(Label);
		}


		private bool HasControl { get; set; }

		private LoopControlType ControlType { get; set; }

		internal void Control(LoopControlType controlType) {
			HasControl = true;
			ControlType = controlType;
		}

		protected override SqlStatement PrepareStatement(IContext context) {
			var loop = CreateNew();
			foreach (var statement in Statements) {
				var prepared = statement.Prepare(context);
				if (prepared == null)
					throw new SqlStatementException("The preparation of a child statement was invalid");

				loop.Statements.Add(prepared);
			}

			return loop;
		}

		protected override async Task ExecuteStatementAsync(StatementContext context) {
			await InitializeAsync(context);

			while (await CanLoopAsync(context)) {
				bool stopLoop = false;

				foreach (var statement in Statements) {
					if (stopLoop)
						break;

					await statement.ExecuteAsync(context);

					if (context.WasTerminated)
						return;

					if (HasControl) {
						if (ControlType == LoopControlType.Exit)
							return;

						if (ControlType == LoopControlType.Continue) {
							stopLoop = true;
						}
					}
				}

				await AfterLoopAsync(context);
			}
		}

		protected virtual Task<bool> CanLoopAsync(StatementContext context) {
			return Task.FromResult(!HasControl || ControlType != LoopControlType.Exit);
		}

		protected virtual Task InitializeAsync(StatementContext context) {
			return Task.CompletedTask;
		}

		protected virtual Task AfterLoopAsync(StatementContext context) {
			return Task.CompletedTask;
		}

		internal void AppendLabelTo(SqlStringBuilder builder) {
			if (!String.IsNullOrEmpty(Label)) {
				builder.AppendFormat("<<{0}>>", Label);
				builder.AppendLine();
			}
		}

		internal void AppendBodyTo(SqlStringBuilder builder) {
			builder.AppendLine("LOOP");
			builder.Indent();

			foreach (var child in Statements) {
				child.AppendTo(builder);
				builder.AppendLine();
			}

			builder.DeIndent();
			builder.Append("END LOOP;");
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			AppendLabelTo(builder);
			AppendBodyTo(builder);
		}
	}
}