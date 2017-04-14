using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Security;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public abstract class SqlStatement : ISqlFormattable, ISqlExpressionPreparable<SqlStatement> {
		public virtual bool CanPrepare => true;

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		protected virtual void AppendTo(SqlStringBuilder builder) {
			
		}

		SqlStatement ISqlExpressionPreparable<SqlStatement>.PrepareExpressions(ISqlExpressionPreparer preparer) {
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
			var preparers = context.Scope.ResolveAll<ISqlExpressionPreparer>();
			var result = this;

			foreach (var preparer in preparers) {
				result = PrepareExpressions(preparer);
			}

			if (CanPrepare)
				result = result.PrepareStatement(context);

			return result;
		}

		private void CheckRequirements(IContext context) {
			var registry = new RequirementCollection();
			Require(registry);

			using (var securityContext = context.Create($"Statement{GetType().Name}.Security")) {
				securityContext.RegisterInstance<IRequirementCollection>(registry);

				securityContext.CheckRequirements();
			}
		}

		public Task ExecuteAsync(IContext context) {
			CheckRequirements(context);

			try {
				return ExecuteStatementAsync(context);
			} catch (SqlStatementException) {
				throw;
			} catch (Exception ex) {
				throw new SqlStatementException("Could not execute the statement because of an error", ex);
			}
		}

		protected abstract Task ExecuteStatementAsync(IContext context);

		public override string ToString() {
			return this.ToSqlString();
		}
	}
}