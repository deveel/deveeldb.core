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
using System.Runtime;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Tables {
	public abstract class DynamicTableContainer : IDbObjectManager {
		~DynamicTableContainer() {
			Dispose(false);
		}

		DbObjectType IDbObjectManager.ObjectType => DbObjectType.Table;

		Task IDbObjectManager.CreateObjectAsync(IDbObjectInfo objInfo) {
			throw new NotSupportedException();
		}

		Task<bool> IDbObjectManager.ObjectExistsAsync(ObjectName objName) {
			return Task.FromResult(FindByName(objName) != -1);
		}

		Task<IDbObjectInfo> IDbObjectManager.GetObjectInfoAsync(ObjectName objectName) {
			var index = FindByName(objectName);
			if (index == -1)
				return Task.FromResult<IDbObjectInfo>(null);

			return Task.FromResult<IDbObjectInfo>(GetTableInfo(index));
		}

		Task<IDbObject> IDbObjectManager.GetObjectAsync(ObjectName objName) {
			var index = FindByName(objName);
			if (index == -1)
				return Task.FromResult<IDbObject>(null);

			return Task.FromResult<IDbObject>(GetTable(index));
		}

		Task<bool> IDbObjectManager.AlterObjectAsync(IDbObjectInfo objInfo) {
			throw new NotSupportedException();
		}

		Task<bool> IDbObjectManager.DropObjectAsync(ObjectName objName) {
			return Task.FromResult(false);
		}

		Task<ObjectName> IDbObjectManager.ResolveNameAsync(ObjectName objName, bool ignoreCase) {
			return Task.FromResult(ResolveName(objName, ignoreCase));
		}

		public abstract ObjectName ResolveName(ObjectName objectName, bool ignoreCase);

		protected abstract TableInfo GetTableInfo(int offset);

		protected abstract ITable GetTable(int offset);

		protected abstract int FindByName(ObjectName tableName);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			
		}
	}
}