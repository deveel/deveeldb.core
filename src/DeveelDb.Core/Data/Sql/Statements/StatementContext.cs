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

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Statements {
	public class StatementContext : Context, IEventSource {
		private Dictionary<string, object> metadata;

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

		IEventSource IEventSource.ParentSource {
			get { return ParentContext.GetEventSource(); }
		}

		IEnumerable<KeyValuePair<string, object>> IEventSource.Metadata => metadata;

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