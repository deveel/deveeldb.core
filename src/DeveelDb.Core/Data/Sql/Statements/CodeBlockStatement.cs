using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Deveel.Data.Serialization;

namespace Deveel.Data.Sql.Statements {
	public abstract class CodeBlockStatement : SqlStatement, ILabeledStatement, IStatementContainer {
		protected CodeBlockStatement() {
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

		public string Label { get; set; }

		public ICollection<SqlStatement> Statements { get; }

		IEnumerable<SqlStatement> IStatementContainer.Statements => Statements;

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
				builder.AppendLine($"<<{Label}>>");
			}

			AppendCodeBlockTo(builder);

			if (Statements.Count > 0) {
				builder.Indent();

				foreach (var statement in Statements) {
					statement.AppendTo(builder);
				}

				builder.DeIndent();
			}
		}

		protected virtual void AppendCodeBlockTo(SqlStringBuilder builder) {
			
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
				}

				base.ClearItems();
			}

			protected override void InsertItem(int index, SqlStatement item) {
				item.Parent = codeBlock;
				base.InsertItem(index, item);
			}

			protected override void RemoveItem(int index) {
				var item = Items[index];
				item.Parent = null;

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