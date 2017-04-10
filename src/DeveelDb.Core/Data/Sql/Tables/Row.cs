using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public sealed class Row {
		private Dictionary<int, SqlObject> values;

		public Row(ITable table, long number) {
			if (table == null)
				throw new ArgumentNullException(nameof(table));

			Table = table;
			RowNumber = number;
			values = new Dictionary<int, SqlObject>();
		}

		private long RowNumber { get; }

		public ITable Table { get; }

		public RowId Id { get; }

		private TableInfo TableInfo => ((TableInfo) Table.ObjectInfo);

		public SqlObject this[int offset] {
			get => GetValue(offset);
			set => SetValue(offset, value);
		}

		public SqlObject this[string columnName] {
			get => GetValue(columnName);
			set => SetValue(columnName, value);
		}

		public SqlObject GetValue(int column) {
			if (column < 0 || column >= TableInfo.Columns.Count)
				throw new ArgumentOutOfRangeException();

			SqlObject value;
			if (!values.TryGetValue(column, out value)) {
				value = Table.GetValue(Id.Number, column);
				values[column] = value;
			}

			return value;
		}

		public SqlObject GetValue(string columnName) {
			if (String.IsNullOrWhiteSpace(columnName))
				throw new ArgumentNullException(nameof(columnName));

			var offset = TableInfo.Columns.IndexOf(columnName);
			if (offset < 0)
				throw new ArgumentException();

			return GetValue(offset);
		}

		public void SetValue(int column, SqlObject value) {
			if (column < 0 || column >= TableInfo.Columns.Count)
				throw new ArgumentOutOfRangeException();

			values[column] = value;
		}

		public void SetValue(string columnName, SqlObject value) {
			if (String.IsNullOrWhiteSpace(columnName))
				throw new ArgumentNullException(nameof(columnName));

			var offset = TableInfo.Columns.IndexOf(columnName);
			if (offset < 0)
				throw new ArgumentException();

			SetValue(offset, value);
		}
	}
}