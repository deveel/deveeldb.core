using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Tables {
	public class TemporaryTable : IVirtualTable, IRootTable {
		private List<SqlObject[]> rows;

		public TemporaryTable(TableInfo tableInfo) {
			TableInfo = TableInfo.ReadOnly(tableInfo);
			rows = new List<SqlObject[]>();
		}

		public TemporaryTable(TableInfo tableInfo, ObjectName alias)
			: this(TableInfo.Alias(tableInfo, alias)) {
		}

		public TemporaryTable(ObjectName tableName, IEnumerable<ColumnInfo> columns)
			: this(MakeTableInfo(tableName, columns)) {
		}

		public TableInfo TableInfo { get; }

		IDbObjectInfo IDbObject.ObjectInfo => TableInfo;

		public long RowCount => rows.Count;

		private static TableInfo MakeTableInfo(ObjectName tableName, IEnumerable<ColumnInfo> columns) {
			var tableInfo = new TableInfo(tableName);
			foreach (var column in columns) {
				tableInfo.Columns.Add(column);
			}
			return tableInfo;
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

		bool IEquatable<ITable>.Equals(ITable other) {
			return this == other;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public IEnumerator<Row> GetEnumerator() {
			return new SimpleRowEnumerator(this);
		}

		IEnumerable<long> IVirtualTable.ResolveRows(int column, IEnumerable<long> rowSet, ITable ancestor) {
			if (ancestor != this)
				throw new ArgumentException("Method routed to wrong accessor");

			return rowSet;
		}

		RawTableInfo IVirtualTable.GetRawTableInfo(RawTableInfo rootInfo) {
			var tableRows = rows.Select((item, index) => (long) index).ToBigArray();
			rootInfo.Add(this, tableRows);
			return rootInfo;
		}

		public SqlObject GetValue(long row, int column) {
			if (row > Int32.MaxValue)
				throw new ArgumentOutOfRangeException("row");

			if (row < 0 || row >= rows.Count)
				throw new ArgumentOutOfRangeException(nameof(row));

			var values = rows[(int) row];
			return values[column];
		}

		public void SetValue(long row, int column, SqlObject value) {
			if (row < 0 || row >= rows.Count)
				throw new ArgumentOutOfRangeException(nameof(row));

			var values = rows[(int) row];
			values[column] = value;
		}

		public void AddRow(SqlObject[] values) {
			if (values.Length != TableInfo.Columns.Count)
				throw new ArgumentException();

			rows.Add(values);
		}

		public int NewRow() {
			rows.Add(new SqlObject[TableInfo.Columns.Count]);
			return rows.Count - 1;
		}

		public static TemporaryTable SingleColumnTable(string columnName, SqlType columnType) {
			var tableInfo = new TableInfo(new ObjectName("single"));
			tableInfo.Columns.Add(new ColumnInfo(columnName, columnType));
			return new TemporaryTable(tableInfo);
		}
	}
}