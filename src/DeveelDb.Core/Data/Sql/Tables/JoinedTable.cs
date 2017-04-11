using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Tables {
	public abstract class JoinedTable : IVirtualTable {
		protected JoinedTable(ITable[] tables) 
			: this(new ObjectName("#VIRTUAL_TABLE#"), tables) {
		}

		protected JoinedTable(ObjectName tableName, ITable[] tables) {
			var tableInfos = tables.Select(x => x.ObjectInfo).Cast<TableInfo>().ToArray();

			TableInfo = new JoinedTableInfo(tableName, tableInfos);
			Tables = tables;
		}

		IDbObjectInfo IDbObject.ObjectInfo => TableInfo;

		public JoinedTableInfo TableInfo { get; }

		TableInfo ITable.TableInfo => TableInfo;

		protected ITable[] Tables { get; }

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

		public abstract long RowCount { get; }

		public virtual SqlObject GetValue(long row, int column) {
			int tableNum = TableInfo.GetTableOffset(column);
			var parentTable = Tables[tableNum];
			var resolvedRow = ResolveTableRow(row, tableNum);
			return parentTable.GetValue(resolvedRow, TableInfo.GetColumnOffset(column));
		}

		protected abstract IEnumerable<long> ResolveTableRows(IEnumerable<long> rowSet, int tableNum);

		protected long ResolveTableRow(long row, int tableNum) {
			return ResolveTableRows(new[] {row}, tableNum).First();
		}

		RawTableInfo IVirtualTable.GetRawTableInfo(RawTableInfo tableInfo) {
			return GetRawTableInfo(tableInfo);
		}

		IEnumerable<long> IVirtualTable.ResolveRows(int column, IEnumerable<long> rowSet, ITable ancestor) {
			return ResolveRows(column, rowSet, ancestor);
		}

		protected virtual IEnumerable<long> ResolveRows(int column, IEnumerable<long> rowSet, ITable ancestor) {
			if (ancestor == this)
				return new BigArray<long>(0);

			int tableNum = TableInfo.GetTableOffset(column);
			var parentTable = Tables[tableNum];

			if (!(parentTable is IVirtualTable))
				throw new InvalidOperationException();

			// Resolve the rows into the parents indices
			var rows = ResolveTableRows(rowSet, tableNum);

			return ((IVirtualTable) parentTable).ResolveRows(TableInfo.GetColumnOffset(column), rows, ancestor);
		}

		

		protected virtual RawTableInfo GetRawTableInfo(RawTableInfo rootInfo) {
			var size = RowCount;
			var allList = new BigArray<long>(size);

			for (int i = 0; i < size; ++i) {
				allList[i] = i;
			}

			return GetRawTableInfo(rootInfo, allList);
		}

		private BigArray<long> CalculateTableRows() {
			var size = RowCount;
			var allList = new BigArray<long>(size);
			for (int i = 0; i < size; ++i) {
				allList[i] = i;
			}
			return allList;
		}

		private RawTableInfo GetRawTableInfo(RawTableInfo info, BigArray<long> rows) {
			if (this is IRootTable) {
				info.Add((IRootTable)this, CalculateTableRows());
			} else {
				for (int i = 0; i < Tables.Length; ++i) {

					// Resolve the rows into the parents indices.
					var newRowSet = ResolveTableRows(rows, i).ToBigArray();

					var table = Tables[i];
					if (table is IRootTable) {
						info.Add((IRootTable)table, newRowSet);
					} else if (table is JoinedTable) {
						((JoinedTable)table).GetRawTableInfo(info, newRowSet);
					}
				}
			}

			return info;
		}

	}
}