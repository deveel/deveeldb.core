using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Indexes;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Indexes {
	public abstract class Index : IDbObject, IDisposable {
		private static readonly BlockIndex<SqlObject, long> EmptyList;
		private static readonly BlockIndex<SqlObject, long> OneList;

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

		public IndexInfo IndexInfo { get; }

		IDbObjectInfo IDbObject.ObjectInfo => IndexInfo;

		public void AttachTo(ITable table) {
			Table = table;
			Columns = IndexInfo.ColumnNames.Select(x => table.TableInfo.Columns.IndexOf(x)).ToArray();
		}

		protected IndexKey GetKey(long row) {
			var values = new SqlObject[Columns.Length];
			for (int i = 0; i < Columns.Length; i++) {
				values[i] = Table.GetValue(row, Columns[i]);
			}

			return new IndexKey(values);
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

			if (rowSetLength <= 250000) {
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

		public abstract void Insert(long row);

		public abstract void Remove(long row);

		public Index Subset(ITable table, int[] columns) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			
		}

		#region IndexKey

		protected struct IndexKey : IComparable<IndexKey>, IEquatable<IndexKey> {
			private readonly SqlObject[] values;

			internal IndexKey(SqlObject[] values) {
				this.values = values;
			}

			public int CompareTo(IndexKey other) {
				int result = 0;
				for (int i = 0; i < values.Length; i++) {
					result = values[i].CompareTo(other.values[i]);
					if (result != 0)
						return result;
				}

				return result;
			}

			public bool Equals(IndexKey other) {
				if (values.Length != other.values.Length)
					return false;

				for (int i = 0; i < values.Length; i++) {
					if (!values[i].Equals(other.values[i]))
						return false;
				}

				return true;
			}
		}

		#endregion

		#region IndexComparer

		private class IndexComparer : IIndexComparer<IndexKey, long> {
			private readonly Index scheme;
			private readonly BigArray<long> rowSet;

			public IndexComparer(Index scheme, BigArray<long> rowSet) {
				this.scheme = scheme;
				this.rowSet = rowSet;
			}

			public int Compare(long index, IndexKey val) {
				var key = scheme.GetKey(rowSet[index]);
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