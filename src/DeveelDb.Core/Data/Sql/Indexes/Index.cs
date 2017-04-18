using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Indexes;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Indexes {
	public abstract class Index : IDbObject, IDisposable {
		private static readonly BlockIndex<SqlObject, long> EmptyList;
		private static readonly BlockIndex<SqlObject, long> OneList;

		private const int OptimalSize = 250000;

		protected Index(IndexInfo indexInfo) {
			if (indexInfo == null)
				throw new ArgumentNullException(nameof(indexInfo));

			IndexInfo = indexInfo;
		}

		static Index() {
			EmptyList = new BlockIndex<SqlObject, long>();
			EmptyList.IsReadOnly = true;
			OneList = new BlockIndex<SqlObject, long>();
			OneList.Add(0);
			OneList.IsReadOnly = true;
		}

		protected int[] Columns { get; private set; }

		protected ITable Table { get; private set; }

		public bool IsAttached { get; private set; }

		public virtual bool IsReadOnly => false;

		public IndexInfo IndexInfo { get; }

		protected IndexKey NullKey => new IndexKey(Columns.Select(x => SqlObject.Null).ToArray());

		IDbObjectInfo IDbObject.ObjectInfo => IndexInfo;

		public void AttachTo(ITable table) {
			Table = table;
			Columns = IndexInfo.ColumnNames.Select(x => table.TableInfo.Columns.IndexOf(x)).ToArray();
			IsAttached = true;
		}

		protected IndexKey GetKey(long row) {
			ThrowIfNotAttached();

			var values = new SqlObject[Columns.Length];
			for (int i = 0; i < Columns.Length; i++) {
				values[i] = Table.GetValue(row, Columns[i]);
			}

			return new IndexKey(values);
		}

		protected void ThrowIfNotAttached() {
			if (!IsAttached)
				throw new InvalidOperationException("The index is not attached to any table");
		}

		protected IEnumerable<long> OrderRows(IEnumerable<long> rows) {
			var rowSet = rows.ToBigArray();

			// The length of the set to order
			var rowSetLength = rowSet.Length;

			// Trivial cases where sorting is not required:
			// NOTE: We use readOnly objects to save some memory.
			if (rowSetLength == 0)
				return EmptyList;
			if (rowSetLength == 1)
				return OneList;

			// This will be 'row set' sorted by its entry lookup.  This must only
			// contain indices to rowSet entries.
			var newSet = new BlockIndex<IndexKey, long>();

			if (rowSetLength <= OptimalSize) {
				// If the subset is less than or equal to 250,000 elements, we generate
				// an array in memory that contains all values in the set and we sort
				// it.  This requires use of memory from the heap but is faster than
				// the no heap use method.
				var subsetList = new BigList<IndexKey>(rowSetLength);
				foreach (var row in rowSet) {
					subsetList.Add(GetKey(row));
				}

				// The comparator we use to sort
				var comparer = new SubsetIndexComparer(subsetList.ToBigArray());

				// Fill new_set with the set { 0, 1, 2, .... , row_set_length }
				for (int i = 0; i < rowSetLength; ++i) {
					var cell = subsetList[i];
					newSet.InsertSort(cell, i, comparer);
				}

			} else {
				// This is the no additional heap use method to sorting the sub-set.

				// The comparator we use to sort
				var comparer = new IndexComparer(this, rowSet);

				// Fill new_set with the set { 0, 1, 2, .... , row_set_length }
				for (int i = 0; i < rowSetLength; ++i) {
					var key = GetKey(rowSet[i]);
					newSet.InsertSort(key, i, comparer);
				}
			}

			return newSet;
		}

		public abstract IEnumerable<long> SelectRange(IndexRange[] ranges);

		public IEnumerable<long> SelectRange(IndexRange range) {
			return SelectRange(new[] { range });
		}

		public IEnumerable<long> SelectAll()
			=> SelectRange(IndexRange.FullRange);

		// NOTE: This will find NULL at start which is probably wrong.  The
		//   first value should be the first non null value.
		public IEnumerable<long> SelectFirst()
			=> SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, IndexRange.FirstInSet,
				RangeFieldOffset.LastValue, IndexRange.FirstInSet));

		public IEnumerable<long> SelectLast()
			=> SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, IndexRange.LastInSet,
				RangeFieldOffset.LastValue, IndexRange.LastInSet));

		public IEnumerable<long> SelectNotNull()
			=> SelectRange(new IndexRange(
				RangeFieldOffset.AfterLastValue, NullKey,
				RangeFieldOffset.LastValue, IndexRange.LastInSet));

		public IEnumerable<long> SelectEqual(IndexKey key) {
			if (key.IsNull)
				return new long[0];

			return SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, key,
				RangeFieldOffset.LastValue, key));
		}

		public IEnumerable<long> SelectNotEqual(IndexKey key) {
			if (key.IsNull)
				return new long[0];

			return SelectRange(new[] {
				new IndexRange(
					RangeFieldOffset.AfterLastValue, key.NullKey,
					RangeFieldOffset.BeforeFirstValue, key),
				new IndexRange(
					RangeFieldOffset.AfterLastValue, key,
					RangeFieldOffset.LastValue, IndexRange.LastInSet)
			});
		}

		public IEnumerable<long> SelectGreater(IndexKey key) {
			if (key.IsNull)
				return new long[0];

			return SelectRange(
				new IndexRange(
					RangeFieldOffset.AfterLastValue, key,
					RangeFieldOffset.LastValue, IndexRange.LastInSet));
		}

		public IEnumerable<long> SelectLess(IndexKey key) {
			if (key.IsNull)
				return new long[0];

			return SelectRange(new IndexRange(
				RangeFieldOffset.AfterLastValue, key.NullKey,
				RangeFieldOffset.BeforeFirstValue, key));
		}

		public IEnumerable<long> SelectGreaterOrEqual(IndexKey key) {
			if (key.IsNull)
				return new long[0];

			return SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, key,
				RangeFieldOffset.LastValue, IndexRange.LastInSet));
		}

		public IEnumerable<long> SelectLessOrEqual(IndexKey key) {
			if (key.IsNull)
				return new long[0];

			return SelectRange(new IndexRange(
				RangeFieldOffset.AfterLastValue, key.NullKey,
				RangeFieldOffset.LastValue, key));
		}

		public IEnumerable<long> SelectBetween(IndexKey key1, IndexKey key2) {
			if (key1.IsNull ||
			    key2.IsNull)
				return new long[0];

			return SelectRange(new IndexRange(
				RangeFieldOffset.FirstValue, key1,
				RangeFieldOffset.BeforeFirstValue, key2));
		}

		public abstract void Insert(long row);

		public abstract void Remove(long row);

		public Index Subset(ITable table, int column) {
			return Subset(table, new[] {column});
		}

		public Index Subset(ITable table, int[] columns) {
			if (table == null)
				throw new ArgumentNullException(nameof(table));
			if (columns.Length > 1)
				throw new NotSupportedException("multi-columns subset not implemented yet");

			// Resolve table rows in this table scheme domain.
			var rowSet = new BigList<long>(table.RowCount);
			foreach (var row in table) {
				rowSet.Add(row.Id.Number);
			}

			var rows = table.ResolveRows(columns[0], rowSet, Table);

			// Generates an IIndex which contains indices into 'rowSet' in
			// sorted order.
			var newSet = OrderRows(rows).ToBigArray();

			// Our 'new_set' should be the same size as 'rowSet'
			if (newSet.Length != rowSet.Count) {
				throw new Exception("Internal sort error in finding sub-set.");
			}

			return CreateSubset(table, columns[0], newSet);
		}

		protected virtual Index CreateSubset(ITable table, int column, IEnumerable<long> rows) {
			var columnName = table.TableInfo.Columns.GetColumnName(column);
			var indexInfo = new IndexInfo(new ObjectName("subset"), table.TableInfo.TableName, new[] {columnName.Name});
			var index = new InsertSearchIndex(indexInfo, rows);
			index.AttachTo(table);
			return index;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			
		}

		#region IndexKey

		#endregion

		#region IndexComparer

		private class IndexComparer : IIndexComparer<IndexKey, long> {
			private readonly Index index;
			private readonly BigArray<long> rowSet;

			public IndexComparer(Index index, BigArray<long> rowSet) {
				this.index = index;
				this.rowSet = rowSet;
			}

			public int Compare(long indexed, IndexKey val) {
				var key = index.GetKey(rowSet[indexed]);
				return key.CompareTo(val);
			}
		}

		#endregion

		#region SubsetIndexComparer

		private class SubsetIndexComparer : IIndexComparer<IndexKey, long> {
			private readonly BigArray<IndexKey> subsetList;

			public SubsetIndexComparer(BigArray<IndexKey> subsetList) {
				this.subsetList = subsetList;
			}

			public int Compare(long index, IndexKey val) {
				return subsetList[index].CompareTo(val);
			}
		}

		#endregion
	}
}