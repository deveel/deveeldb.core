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
using System.Threading.Tasks;

using Deveel.Data.Serialization;

namespace Deveel.Data.Sql.Statements {
	public class CodeBlockStatement : SqlStatement, ILabeledStatement, IStatementContainer {
		public CodeBlockStatement() 
			: this((string)null) {
		}

		public CodeBlockStatement(string label) {
			Label = label;
			Statements = new StatementCollection(this);
		}

		protected CodeBlockStatement(SerializationInfo info)
			: base(info) {
			Label = info.GetString("label");
			
			Statements = new StatementCollection(this);
			var statements = info.GetValue<SqlStatement[]>("statements");

			foreach (var statement in statements) {
				Statements.Add(statement);
			}
		}

		public string Label { get; }

		public ICollection<SqlStatement> Statements { get; }

		IEnumerable<SqlStatement> IStatementContainer.Statements => Statements;

		protected override StatementContext CreateContext(IContext parent, string name) {
			return new BlockStatementContext(parent, name, this);
		}

		protected override SqlStatement PrepareStatement(IContext context) {
			var block = new CodeBlockStatement(Label);

			foreach (var statement in Statements) {
				var prepared = statement.Prepare(context);
				block.Statements.Add(prepared);
			}

			return block;
		}

		protected override async Task ExecuteStatementAsync(StatementContext context) {
			foreach (var statement in Statements) {
				await statement.ExecuteAsync(context);
			}
		}

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("label", Label);

			var statements = Statements.ToArray();
			foreach (var statement in statements) {
				statement.Parent = null;
			}

			info.SetValue("statements", statements);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			if (!String.IsNullOrWhiteSpace(Label)) {
				builder.AppendFormat("<<{0}>>", Label);
				builder.AppendLine();
			}

			builder.AppendLine("BEGIN");
			builder.Indent();

			foreach (var statement in Statements) {
				statement.AppendTo(builder);
				builder.AppendLine();
			}

			builder.DeIndent();
			builder.Append("END;");
		}

		#region StatementCollection

		class StatementCollection : Collection<SqlStatement> {
			private readonly CodeBlockStatement codeBlock;

			public StatementCollection(CodeBlockStatement codeBlock) {
				this.codeBlock = codeBlock;
			}

			protected override void ClearItems() {
				foreach (var statement in Items) {
					statement.Parent = null;
					statement.Next = null;
					statement.Previous = null;
				}

				base.ClearItems();
			}

			protected override void InsertItem(int index, SqlStatement item) {
				item.Parent = codeBlock;

				if (index > 0) {
					item.Previous = Items[index - 1];
					Items[index - 1].Next = item;
				}

				if (index < Items.Count) {
					item.Next = Items[index + 1];
					Items[index + 1].Previous = item;
				}

				base.InsertItem(index, item);
			}

			protected override void RemoveItem(int index) {
				var item = Items[index];
				item.Parent = null;

				if (index > 0) {
					if (Items.Count > 2) {
						Items[index - 1].Next = Items[index + 1];
					} else {
						Items[index - 1].Next = null;
					}
				}

				if (index < Items.Count) {
					if (Items.Count > 2) {
						Items[index + 1].Previous = Items[index - 1];
					}
				}

				item.Next = null;
				item.Previous = null;

				base.RemoveItem(index);
			}

			protected override void SetItem(int index, SqlStatement item) {
				item.Parent = codeBlock;
				base.SetItem(index, item);
			}
		}

		#endregion
	}
}