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