using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public class LimitedTable : FilterTable {
		private readonly long offset;
		private readonly long count;

		public LimitedTable(ITable table, long offset, long count)
			: base(table) {
			if (offset < 0)
				offset = 0;

			if (offset + count > table.RowCount)
				throw new ArgumentException();

			this.offset = offset;
			this.count = count;
		}

		public override long RowCount => NormalizeCount(base.RowCount);

		private long NormalizeRow(long rowNumber) {
			rowNumber += offset;
			return rowNumber;
		}

		private long NormalizeCount(long rowCount) {
			rowCount -= offset;
			return System.Math.Min(rowCount, count);
		}

		public override SqlObject GetValue(long row, int column) {
			return base.GetValue(NormalizeRow(row), column);
		}

		public override IEnumerator<Row> GetEnumerator() {
			return new Enumerator(this);
		}

		#region Enumerator

		class Enumerator : IEnumerator<Row> {
			private long offset = -1;
			private LimitedTable table;

			public Enumerator(LimitedTable table) {
				this.table = table;
			}

			public void Dispose() {
				table = null;
			}

			public bool MoveNext() {
				return ++offset < table.RowCount;
			}

			public void Reset() {
				offset = -1;
			}

			public Row Current {
				get { return new Row(table, offset); }
			}

			object IEnumerator.Current {
				get { return Current; }
			}
		}

		#endregion

	}
}