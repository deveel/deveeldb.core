using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Tables {
	public sealed class RawTableInfo {
		private readonly List<RawTableItem> tableItems;

		private RawTableInfo(IEnumerable<RawTableItem> items) {
			tableItems = new List<RawTableItem>();

			if (items != null)
				tableItems.AddRange(items);
		}

		public RawTableInfo()
			: this(null) {
		}

		public void Add(IRootTable table, BigArray<long> rowSet) {
			tableItems.Add(new RawTableItem(table, rowSet));
		}

		public IRootTable[] Tables => tableItems.Select(x => x.Table).ToArray();

		public BigArray<long>[] Rows => tableItems.Select(x => x.Rows).ToArray();

		private RawTableItem[] SortItems() {
			var list = new RawTableItem[tableItems.Count];
			tableItems.CopyTo(list);
			Array.Sort(list);
			return list;
		}


		#region RawTableItem

		class RawTableItem : IComparable<RawTableItem> {
			public RawTableItem(IRootTable table)
				: this(table, new BigArray<long>(0)) {
			}

			public RawTableItem(IRootTable table, BigArray<long> rows) {
				Table = table;
				Rows = rows;
			}

			public IRootTable Table { get; private set; }

			public BigArray<long> Rows { get; private set; }

			public int CompareTo(RawTableItem other) {
				return Table.GetHashCode() - other.Table.GetHashCode();
			}
		}

		#endregion

		#region RawRowItem

		class RawRowItem : IComparable<RawRowItem> {
			public RawRowItem(BigArray<long> values) {
				RowValues = values;
			}

			public BigArray<long> RowValues { get; private set; }

			public int CompareTo(RawRowItem other) {
				var size = RowValues.Length;
				for (var i = 0; i < size; ++i) {
					var v1 = RowValues[i];
					var v2 = other.RowValues[i];
					if (v1 != v2) {
						return (int) (v1 - v2);
					}
				}
				return 0;
			}
		}

		#endregion

	}
}