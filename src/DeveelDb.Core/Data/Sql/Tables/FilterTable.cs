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

namespace Deveel.Data.Sql.Tables {
	public class FilterTable : IVirtualTable {
		public FilterTable(ITable parent) {
			Parent = parent;
		}

		protected ITable Parent { get; }

		public virtual TableInfo TableInfo => Parent.TableInfo;

		public virtual long RowCount => Parent.RowCount;

		public virtual SqlObject GetValue(long row, int column) {
			return Parent.GetValue(row, column);
		}

		IDbObjectInfo IDbObject.ObjectInfo => TableInfo;

		public virtual IEnumerator<Row> GetEnumerator()
			=> Parent.GetEnumerator();

		int IComparable.CompareTo(object obj) {
			throw new NotSupportedException();
		}

		int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
			throw new NotSupportedException();
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		IEnumerable<long> IVirtualTable.ResolveRows(int column, IEnumerable<long> rowSet, ITable ancestor) {
			return ResolveRows(column, rowSet, ancestor);
		}

		protected virtual IEnumerable<long> ResolveRows(int column, IEnumerable<long> rows, ITable ancestor) {
			if (ancestor == this || ancestor == Parent)
				return rows;

			if (!(Parent is IVirtualTable))
				throw new InvalidOperationException();

			return ((IVirtualTable) Parent).ResolveRows(column, rows, ancestor);
		}

		RawTableInfo IVirtualTable.GetRawTableInfo(RawTableInfo rootInfo) {
			return GetRawTableInfo(rootInfo);
		}

		protected virtual RawTableInfo GetRawTableInfo(RawTableInfo rootInfo) {
			return Parent.GetRawTableInfo(rootInfo);
		}
	}
}