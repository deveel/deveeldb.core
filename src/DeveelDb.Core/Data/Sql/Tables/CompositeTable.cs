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
using System.Threading.Tasks;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Indexes;

namespace Deveel.Data.Sql.Tables {
	public sealed class CompositeTable : TableBase, IRootTable {
		private readonly ITable mainTable;
		private readonly ITable[] composites;

		private readonly BigArray<long>[] rowIndexes;
		private readonly Index[] columnIndexes;

		public CompositeTable(ITable mainTable, ITable[] composites, CompositeFunction function, bool all) {
			this.mainTable = mainTable;
			this.composites = composites;

			columnIndexes = new Index[mainTable.TableInfo.Columns.Count];
			int size = composites.Length;
			rowIndexes = new BigArray<long>[size];

			if (function == CompositeFunction.Union) {
				// Include all row sets in all tables
				for (int i = 0; i < size; ++i) {
					rowIndexes[i] = composites[i].SelectAllRows().ToBigArray();
				}

				RemoveDuplicates(all);
			} else {
				throw new NotSupportedException($"The composite function '{function.ToString().ToUpperInvariant()}' is not supported (yet).");
			}

		}

		public override TableInfo TableInfo => mainTable.TableInfo;

		public override long RowCount => rowIndexes.Sum(x => x.Length);

		private void RemoveDuplicates(bool all) {
			if (!all)
				throw new NotImplementedException();
		}

		public override IEnumerator<Row> GetEnumerator() {
			return new SimpleRowEnumerator(this);
		}

		bool IEquatable<ITable>.Equals(ITable other) {
			return this == other;
		}

		protected override RawTableInfo GetRawTableInfo(RawTableInfo rootInfo) {
			var rows = this.Select(x => x.Id.Number).ToBigList();
			rootInfo.Add(this, rows);
			return rootInfo;
		}

		protected override IEnumerable<long> ResolveRows(int column, IEnumerable<long> rows, ITable ancestor) {
			if (ancestor != this)
				throw new InvalidOperationException();

			return rows;
		}

		protected override Index GetColumnIndex(int column, int originalColumn, ITable ancestor) {
			var index = columnIndexes[column];
			if (index == null) {
				var indexInfo = TableInfo.CreateColumnIndexInfo(column);
				index = new BlindSearchIndex(indexInfo);
				index.AttachTo(this);

				columnIndexes[column] = index;
			}

			// If we are getting a scheme for this table, simple return the information
			// from the column_trees Vector.
			if (ancestor == this)
				return index;

			// Otherwise, get the scheme to calculate a subset of the given scheme.
			return index.Subset(ancestor, originalColumn);
		}

		public override Task<SqlObject> GetValueAsync(long row, int column) {
			for (int i = 0; i < rowIndexes.Length; ++i) {
				var list = rowIndexes[i];
				var sz = list.Length;
				if (row < sz)
					return composites[i].GetValueAsync(list[row], column);

				row -= sz;
			}

			throw new ArgumentOutOfRangeException("row", $"Row '{row}' out of range.");
		}
	}
}