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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Indexes;

namespace Deveel.Data.Sql.Tables {
	public abstract class DynamicTable : ITable {
		IDbObjectInfo IDbObject.ObjectInfo => TableInfo;

		int IComparable.CompareTo(object obj) {
			throw new NotSupportedException();
		}

		int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
			throw new NotSupportedException();
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return false;
		}

		public IEnumerator<Row> GetEnumerator() {
			return new SimpleRowEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		void ISerializable.GetObjectData(SerializationInfo info) {
			throw new NotSupportedException();
		}

		public abstract TableInfo TableInfo { get; }

		public abstract long RowCount { get; }

		public abstract Task<SqlObject> GetValueAsync(long row, int column);

		public Index GetColumnIndex(int column) {
			var indexInfo = TableInfo.CreateColumnIndexInfo(column);
			var index = new BlindSearchIndex(indexInfo);
			index.AttachTo(this);
			return index;
		}
	}
}