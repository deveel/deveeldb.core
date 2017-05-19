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

namespace Deveel.Data.Sql.Tables {
	public class TransientTableManager : IDbObjectManager {
		private Dictionary<ObjectName, ITable> tables;
		private Dictionary<ObjectName, ObjectName> tableNames;

		public TransientTableManager() {
			tables = new Dictionary<ObjectName, ITable>();
			tableNames = new Dictionary<ObjectName, ObjectName>(ObjectNameComparer.IgnoreCase);
		}


		DbObjectType IDbObjectManager.ObjectType => DbObjectType.Table;

		Task IDbObjectManager.CreateObjectAsync(IDbObjectInfo objInfo) {
			if (!(objInfo is TableInfo))
				throw new ArgumentException();

			return CreateTableAsync((TableInfo)objInfo);
		}

		public Task CreateTableAsync(TableInfo tableInfo) {
			if (tables.ContainsKey(tableInfo.TableName))
				throw new ArgumentException();

			var tempTable = new TemporaryTable(tableInfo);
			tables[tableInfo.TableName] = tempTable;
			tableNames[tableInfo.TableName] = tableInfo.TableName;
			return Task.CompletedTask;
		}

		Task<bool> IDbObjectManager.ObjectExistsAsync(ObjectName objName) {
			return Task.FromResult(TableExists(objName));
		}

		Task<IDbObjectInfo> IDbObjectManager.GetObjectInfoAsync(ObjectName objectName) {
			return Task.FromResult<IDbObjectInfo>(GetTableInfo(objectName));
		}

		public TableInfo GetTableInfo(ObjectName objectName) {
			ITable table;
			if (!tables.TryGetValue(objectName, out table))
				throw new InvalidOperationException();

			return table.TableInfo;
		}

		public bool TableExists(ObjectName tableName) {
			return tables.ContainsKey(tableName);
		}

		async Task<IDbObject> IDbObjectManager.GetObjectAsync(ObjectName objName) {
			return await GetTableAsync(objName);
		}

		public Task<ITable> GetTableAsync(ObjectName tableName) {
			ITable table;
			if (!tables.TryGetValue(tableName, out table))
				return Task.FromResult<ITable>(null);

			return Task.FromResult(table);
		}

		Task<bool> IDbObjectManager.AlterObjectAsync(IDbObjectInfo objInfo) {
			throw new NotImplementedException();
		}

		Task<bool> IDbObjectManager.DropObjectAsync(ObjectName objName) {
			return DropTableAsync(objName);
		}

		public Task<bool> DropTableAsync(ObjectName tableName) {
			ITable table;
			if (!tables.TryGetValue(tableName, out table))
				return Task.FromResult(false);

			tableNames.Remove(table.TableInfo.TableName);
			return Task.FromResult(tables.Remove(tableName));
		}

		public Task<ObjectName> ResolveNameAsync(ObjectName tableName, bool ignoreCase) {
			if (ignoreCase) {
				if (!tableNames.TryGetValue(tableName, out tableName))
					return Task.FromResult<ObjectName>(null);
			}

			if (!tables.ContainsKey(tableName))
				return Task.FromResult<ObjectName>(null);

			return Task.FromResult(tableName);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (tables != null)
					tables.Clear();
				if (tableNames != null)
					tableNames.Clear();
			}

			tableNames = null;
			tables = null;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}