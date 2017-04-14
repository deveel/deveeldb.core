using System;
using System.Collections.Generic;

using Deveel.Data.Indexes;

namespace Deveel.Data.Sql.Indexes {
	public sealed class InsertSearchIndex : CollatedSearchIndex {
		private IIndex<IndexKey, long> list;
		private IIndexComparer<IndexKey, long> comparer;

		public InsertSearchIndex(IndexInfo indexInfo) 
			: this(indexInfo, null) {
		}

		public InsertSearchIndex(IndexInfo indexInfo, IEnumerable<long> rows) 
			: base(indexInfo) {
			comparer = new IndexComparer(this);
			list = new BlockIndex<IndexKey, long>();

			if (rows != null) {
				foreach (var row in rows) {
					list.Add(row);
				}
			}
		}

		private InsertSearchIndex(InsertSearchIndex source, bool readOnly)
			: this(source.IndexInfo, null) {
			AttachTo(source.Table);
			IsReadOnly = readOnly;
		}

		public override bool IsReadOnly { get; }

		protected override long RowCount => list.Count;

		protected override IndexKey First => GetKey(list[0]);

		protected override IndexKey Last => GetKey(list[list.Count - 1]);

		public override void Insert(long row) {
			ThrowIfReadOnly();

			var value = GetKey(row);
			list.InsertSort(value, row, comparer);
		}

		public override void Remove(long row) {
			ThrowIfReadOnly();

			var value = GetKey(row);
			var removed = list.RemoveSort(value, row, comparer);

			if (removed != row)
				throw new InvalidOperationException($"Could not remove the requested row ({row})");
		}

		protected override IEnumerable<long> AddRange(long start, long end, IEnumerable<long> input) {
			var result = new BigList<long>();
			if (input != null)
				result.AddRange(input);

			using (var en = list.GetEnumerator(start, end)) {
				while (en.MoveNext()) {
					result.Add(en.Current);
				}
			}

			return result;
		}

		protected override long SearchFirst(IndexKey value) {
			return list.SearchFirst(value, comparer);
		}

		protected override long SearchLast(IndexKey value) {
			return list.SearchLast(value, comparer);
		}

		#region IndexComparerImpl

		private class IndexComparer : IIndexComparer<IndexKey, long> {
			private readonly InsertSearchIndex columnIndex;

			public IndexComparer(InsertSearchIndex columnIndex) {
				this.columnIndex = columnIndex;
			}

			private int InternalCompare(long index, IndexKey value) {
				var key = columnIndex.GetKey(index);
				return key.CompareTo(value);
			}

			public int Compare(long index, IndexKey val) {
				return InternalCompare(index, val);
			}
		}

		#endregion
	}
}