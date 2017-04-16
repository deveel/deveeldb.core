using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Sql.Indexes;

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

		Index IVirtualTable.GetColumnIndex(int column, int originalColumn, ITable ancestor) {
			return GetColumnIndex(column, originalColumn, ancestor);
		}

		protected virtual Index GetColumnIndex(int column, int originalColumn, ITable ancestor) {
			return GetColumnIndex(column);
		}

		public virtual Index GetColumnIndex(int column) {
			return GetColumnIndex(column, column, this);
		}

		public abstract Task<SqlObject> GetValueAsync(long row, int column);

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