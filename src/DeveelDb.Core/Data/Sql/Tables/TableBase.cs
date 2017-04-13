using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public abstract class TableBase : IVirtualTable, IDisposable {
		public abstract TableInfo TableInfo { get; }

		IDbObjectInfo IDbObject.ObjectInfo => TableInfo;

		public abstract long RowCount { get; }

		public abstract IEnumerator<Row> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		IEnumerable<long> IVirtualTable.ResolveRows(int column, IEnumerable<long> rows, ITable ancestor) {
			return ResolveRows(column, rows, ancestor);
		}

		protected abstract IEnumerable<long> ResolveRows(int column, IEnumerable<long> rows, ITable ancestor);

		protected abstract RawTableInfo GetRawTableInfo(RawTableInfo rootInfo);

		RawTableInfo IVirtualTable.GetRawTableInfo(RawTableInfo rootInfo) {
			return GetRawTableInfo(rootInfo);
		}

		public abstract SqlObject GetValue(long row, int column);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
		}

		int IComparable.CompareTo(object obj) {
			throw new NotSupportedException();
		}

		int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
			throw new NotSupportedException();
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return false;
		}
	}
}