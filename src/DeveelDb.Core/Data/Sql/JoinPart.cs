﻿// 
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

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql {
	public sealed class JoinPart {
		private JoinPart(JoinType joinType, ObjectName tableName, SqlQueryExpression query, SqlExpression onExpression) {
			JoinType = joinType;
			TableName = tableName;
			Query = query;
			OnExpression = onExpression;
		}

		internal JoinPart(JoinType joinType, ObjectName tableName, SqlExpression onExpression)
			: this(joinType, tableName, (SqlQueryExpression) null, onExpression) {
			if (tableName == null)
				throw new ArgumentNullException(nameof(tableName));
		}

		internal JoinPart(JoinType joinType, SqlQueryExpression query, SqlExpression onExpression)
			: this(joinType, (ObjectName) null, query, onExpression) {
			if (query == null)
				throw new ArgumentNullException(nameof(query));
		}

		public SqlExpression OnExpression { get; }

		public ObjectName TableName { get; }

		public SqlQueryExpression Query { get; }

		public JoinType JoinType { get; }

		public bool IsTable => TableName != null;

		public bool IsQuery => Query != null;
	}
}