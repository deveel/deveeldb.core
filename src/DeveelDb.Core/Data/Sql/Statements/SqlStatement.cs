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
using Deveel.Data.Security;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public abstract class SqlStatement : ISqlFormattable, ISqlExpressionPreparable<SqlStatement> {
		public virtual bool CanPrepare => true;

		protected virtual string Name {
			get {
				var name = GetType().Name;
				if (name.EndsWith("Statenment", StringComparison.OrdinalIgnoreCase))
					name = name.Substring(0, name.Length - 10);

				return name;
			}
		}

		public LocationInfo Location { get; set; }

		protected virtual StatementContext CreateContext(IContext parent) {
			return new StatementContext(parent, Name, this);
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		protected virtual void AppendTo(SqlStringBuilder builder) {
			
		}

		internal void CollectMetadata(IDictionary<string, object> data) {
			GetMetadata(data);
		}

		protected virtual void GetMetadata(IDictionary<string, object> data) {
			
		}

		SqlStatement ISqlExpressionPreparable<SqlStatement>.Prepare(ISqlExpressionPreparer preparer) {
			return PrepareExpressions(preparer);
		}

		protected virtual SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			return this;
		}

		protected virtual SqlStatement PrepareStatement(IContext context) {
			return this;
		}

		protected virtual void Require(IRequirementCollection requirements) {
			
		}

		public SqlStatement Prepare(IContext context) {
			using (var statementContext = CreateContext(context)) {
				var preparers = context.Scope.ResolveAll<ISqlExpressionPreparer>();
				var result = this;

				foreach (var preparer in preparers) {
					result = PrepareExpressions(preparer);
				}

				if (CanPrepare)
					result = result.PrepareStatement(statementContext);

				return result;
			}
		}

		private async Task CheckRequirements(IContext context) {
			context.Debug(-1, "Collecting security requirements");

			var registry = new RequirementCollection();
			Require(registry);

			using (var securityContext = context.Create($"Statement{GetType().Name}.Security",
				scope => scope.RegisterInstance<IRequirementCollection>(registry))) {
				context.Debug(-1, "Check security requirements");

				await securityContext.CheckRequirementsAsync();
			}
		}

		public async Task<SqlStatementResult> ExecuteAsync(IContext context) {
			using (var statementContext = CreateContext(context)) {

				await CheckRequirements(statementContext);

				try {
					return await ExecuteStatementAsync(statementContext);
				} catch (SqlStatementException) {

					throw;
				} catch (Exception ex) {
					statementContext.Error(-1, "Could not execute the statement", ex);
					throw new SqlStatementException("Could not execute the statement because of an error", ex);
				}
			}
		}

		protected abstract Task<SqlStatementResult> ExecuteStatementAsync(IContext context);

		public override string ToString() {
			return this.ToSqlString();
		}
	}
}