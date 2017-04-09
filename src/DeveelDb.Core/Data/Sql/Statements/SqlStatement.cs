using System;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public abstract class SqlStatement : ISqlFormattable, ISqlExpressionPreparable {
		public virtual bool CanPrepare => true;

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		protected virtual void AppendTo(SqlStringBuilder builder) {
			
		}

		object ISqlExpressionPreparable.PrepareExpressions(ISqlExpressionPreparer preparer) {
			return PrepareExpressions(preparer);
		}

		protected virtual SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			return this;
		}

		protected virtual SqlStatement PrepareStatement(IContext context) {
			return this;
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

		public Task ExecuteAsync(IRequest context) {
			try {
				return ExecuteStatementAsync(context);
			} catch (SqlStatementException) {
				throw;
			} catch (Exception ex) {
				throw new SqlStatementException("Could not execute the statement because of an error", ex);
			}
		}

		protected abstract Task ExecuteStatementAsync(IRequest context);

		public override string ToString() {
			return this.ToSqlString();
		}
	}
}