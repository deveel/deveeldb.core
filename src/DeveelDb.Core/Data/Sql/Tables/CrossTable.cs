using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Tables {
	public class CrossTable : JoinedTable {
		private readonly long leftRowCount;
		private readonly long rightRowCount;
 
		private readonly IEnumerable<long> leftRows;
		private readonly IEnumerable<long> rightRows;

		private readonly bool leftIsSimpleEnum;
		private readonly bool rightIsSimpleEnum;

		public CrossTable(ITable left, ITable right)
			: base(new[] {left, right}) {

			leftRowCount = left.RowCount;
			rightRowCount = right.RowCount;

			leftIsSimpleEnum = left.GetEnumerator() is SimpleRowEnumerator;
			rightIsSimpleEnum = right.GetEnumerator() is SimpleRowEnumerator;

			leftRows = !leftIsSimpleEnum ? left.Select(x => x.Id.Number) : null;
			rightRows = !rightIsSimpleEnum ? right.Select(x => x.Id.Number) : null;
		}

		public override long RowCount => leftRowCount * rightRowCount;

		private long GetRightRowIndex(long rowIndex) {
			if (rightIsSimpleEnum)
				return rowIndex;

			return rightRows.ElementAt(rowIndex);
		}

		private long GetLeftRowIndex(long rowIndex) {
			if (leftIsSimpleEnum)
				return rowIndex;

			return leftRows.ElementAt(rowIndex);
		}

		protected override IEnumerable<long> ResolveTableRows(IEnumerable<long> rowSet, int tableNum) {
			var rowList = rowSet.ToBigArray();
			bool pickRightTable = (tableNum == 1);
			for (long n = rowList.Length - 1; n >= 0; --n) {
				var row = rowList[n];

				// Reverse map row index to parent domain
				long parentRow;
				if (pickRightTable) {
					parentRow = GetRightRowIndex(row % rightRowCount);
				} else {
					parentRow = GetLeftRowIndex(row / rightRowCount);
				}
				rowList[n] = parentRow;
			}

			return rowList;
		}
	}
}