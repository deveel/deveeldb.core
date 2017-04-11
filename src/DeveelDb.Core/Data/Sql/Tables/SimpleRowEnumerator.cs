using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public class SimpleRowEnumerator : IEnumerator<Row> {
		private long offset;
		private long rowCount;

		public SimpleRowEnumerator(ITable table) {
			Table = table;
			Reset();
		}

		public ITable Table { get; }


		public bool MoveNext() {
			return ++offset < rowCount;
		}

		public void Reset() {
			offset = -1;
			rowCount = Table.RowCount;
		}

		public Row Current => new Row(Table, offset);

		object IEnumerator.Current {
			get { return Current; }
		}

		public void Dispose() {
			
		}
	}
}