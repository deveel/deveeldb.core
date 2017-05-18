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

using Deveel.Data.Diagnostics;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public class StatementContext : Context, IEventSource {
		private Dictionary<string, object> metadata;

		public StatementContext(IContext parent, SqlStatement statement) 
			: this(parent, statement.StatementName, statement) {
		}

		public StatementContext(IContext parent, string name, SqlStatement statement) 
			: base(parent, name) {
			if (statement == null)
				throw new ArgumentNullException(nameof(statement));

			Statement = statement;
			EnsureMetadata();
		}

		public SqlStatement Statement { get; }

		private void EnsureMetadata() {
			if (metadata == null) {
				metadata = new Dictionary<string, object>();

				GetMetadata(metadata);
			}
		}

		IEventSource IEventSource.ParentSource => ParentContext.GetEventSource();

		IEnumerable<KeyValuePair<string, object>> IEventSource.Metadata => metadata;

		public IStatementResult Result { get; private set; }

		public bool HasResult { get; private set; }

		private bool WasTerminated { get; set; }

		private void Terminate() {
			WasTerminated = true;

			if (ParentContext != null &&
				ParentContext is StatementContext) {
				var context = (StatementContext) ParentContext;
				context.Result = Result;
				context.HasResult = HasResult;
				context.Terminate();
			}
		}

		private void ThrowIfTerminated() {
			if (WasTerminated)
				throw new InvalidOperationException("The statement context was terminated");
		}


		public void SetResult(IStatementResult result) {
			ThrowIfTerminated();

			Result = result;
			HasResult = true;
		}

		public void SetResult(SqlExpression value)
			=> SetResult(new StatementExpressionResult(value));

		public void Return(IStatementResult result) {
			SetResult(result);
			Terminate();
		}

		public void Return(SqlExpression value)
			=> Return(new StatementExpressionResult(value));

		public async Task TransferAsync(string label) {
			ThrowIfTerminated();

			if (String.IsNullOrEmpty(label))
				throw new ArgumentNullException(nameof(label));

			var statement = FindInTree(Statement, label);
			if (statement == null)
				throw new SqlStatementException($"Could not find any block labeled '{label}' in the execution tree.");

			using (var block = NewBlock(statement)) {
				await statement.ExecuteAsync(block);

				if (block.HasResult && !WasTerminated)
					SetResult(block.Result);
			}
		}

		private StatementContext NewBlock(SqlStatement statement) {
			return new StatementContext(this, statement.StatementName, statement);
		}

		private SqlStatement FindInTree(SqlStatement root, string label) {
			var statement = root;
			while (statement != null) {
				if (statement is ILabeledStatement) {
					var block = (ILabeledStatement) statement;
					if (String.Equals(label, block.Label, StringComparison.Ordinal))
						return statement;
				}

				if (statement is IStatementContainer) {
					var container = (IStatementContainer)statement;
					foreach (var child in container.Statements) {
						var found = FindInTree(child, label);
						if (found != null)
							return found;
					}
				}

				statement = statement.Parent;
			}

			return null;
		}


		protected virtual void GetMetadata(IDictionary<string, object> data) {
			if (Statement.Location != null) {
				data["statement.line"] = Statement.Location.Line;
				data["statement.column"] = Statement.Location.Column;
			}

			Statement.CollectMetadata(data);
			data["statement.sql"] = Statement.ToSqlString();
		}
	}
}