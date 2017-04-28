using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

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