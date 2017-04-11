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