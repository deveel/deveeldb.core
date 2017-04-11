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
			if (!(Parent is IVirtualTable))
				throw new InvalidOperationException();

			return ((IVirtualTable) Parent).GetRawTableInfo(rootInfo);
		}
	}
}