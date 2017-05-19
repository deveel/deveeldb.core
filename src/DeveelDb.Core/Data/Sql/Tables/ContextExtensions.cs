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
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Diagnostics;

namespace Deveel.Data.Sql.Tables {
	public static class ContextExtensions {
		public static Task<bool> TableExistsAsync(this IContext context, ObjectName tableName) {
			return context.ObjectExistsAsync(DbObjectType.Table, tableName);
		}

		public static async Task<ITable> GetTableAsync(this IContext context, ObjectName tableName) {
			return (await context.GetObjectAsync(DbObjectType.Table, tableName)) as ITable;
		}

		public static async Task<TableInfo> GetTableInfoAsync(this IContext context, ObjectName tableName) {
			return (await context.GetObjectInfoAsync(DbObjectType.Table, tableName)) as TableInfo;
		}
	}
}