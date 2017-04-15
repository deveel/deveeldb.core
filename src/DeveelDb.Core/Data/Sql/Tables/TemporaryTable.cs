using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Sql.Indexes;

namespace Deveel.Data.Sql.Tables {
	public class TemporaryTable : DataTableBase {
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

		public override TableInfo TableInfo { get; }

		public override long RowCount => rows.Count;

		private static TableInfo MakeTableInfo(ObjectName tableName, IEnumerable<ColumnInfo> columns) {
			var tableInfo = new TableInfo(tableName);
			foreach (var column in columns) {
				tableInfo.Columns.Add(column);
			}
			return tableInfo;
		}

		public override IEnumerator<Row> GetEnumerator() {
			return new SimpleRowEnumerator(this);
		}

		protected override RawTableInfo GetRawTableInfo(RawTableInfo rootInfo) {
			var tableRows = rows.Select((item, index) => (long) index).ToBigArray();
			rootInfo.Add(this, tableRows);
			return rootInfo;
		}

		public override SqlObject GetValue(long row, int column) {
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

		public void BuildIndex() {
			SetupIndexes(typeof(BlindSearchIndex));

			for (int i = 0; i < RowCount; i++) {
				AddRowToIndex(i);
			}
		}
	}
}