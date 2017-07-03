using System;

using Deveel.Data.Diagnostics;
using Deveel.Data.Services;
using Deveel.Data.Sql;

namespace Deveel.Data {
	class QueryObj : EventSource, IQuery {
		private IScope scope;

		public QueryObj(ISession session, SqlQuery sourceQuery) {
			scope = session.Scope.OpenScope("query");
			scope.RegisterInstance<SqlQuery>(sourceQuery, null);
			scope = scope.AsReadOnly();

			SourceQuery = sourceQuery;
			Session = session;
		}

		~QueryObj() {
			Dispose(false);
		}

		IContext IContext.ParentContext => Session;

		string IContext.ContextName => "query";

		IScope IContext.Scope => scope;

		public SqlQuery SourceQuery { get; }

		public ISession Session { get; }

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (scope != null)
					scope.Dispose();
			}

			scope = null;
		}
	}
}