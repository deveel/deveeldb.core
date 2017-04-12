using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Indexes {
	public abstract class CollatedSearchIndex : Index {
		protected CollatedSearchIndex(IndexInfo indexInfo) 
			: base(indexInfo) {
		}

		protected virtual long RowCount => Table.RowCount;

		protected virtual IndexKey First => GetKey(0);

		protected virtual IndexKey Last => GetKey(RowCount - 1);

		private void AssertNotReadOnly() {
			if (IsReadOnly)
				throw new InvalidOperationException("The index is read-only");
		}

		public override void Insert(long row) {
			AssertNotReadOnly();
		}

		public override void Remove(long row) {
			AssertNotReadOnly();
		}

		protected abstract long SearchFirst(IndexKey value);

		protected abstract long SearchLast(IndexKey value);

		protected virtual IEnumerable<long> AddRange(int start, int end, IEnumerable<long> input) {
			var list = new BigList<long>((end - start) + 2);
			if (input != null)
				list.AddRange(input);

			for (int i = start; i <= end; ++i) {
				list.Add(i);
			}

			return list;
		}
	}
}