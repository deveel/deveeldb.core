// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


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