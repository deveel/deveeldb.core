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

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public static class ContextExtensions {
		public static void TableEvent(this IContext context, int eventId, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableEvent>(eventId, commitId, tableId, tableName);

		public static void TableAccessed(this IContext context, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableAccessEvent>(commitId, tableId, tableName);

		public static void TableSelected(this IContext context, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableSelectEvent>(commitId, tableId, tableName);

		public static void TableCreated(this IContext context, long commitId, int tableId, ObjectName tableName)
			=> context.RegisterEvent<TableCreatedEvent>(commitId, tableId, tableName);


	}
}