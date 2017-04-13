using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Sql.Tables {
	public abstract class DataTableBase : TableBase, IRootTable {
		bool IEquatable<ITable>.Equals(ITable table) {
			return this == table;
		}

		protected override IEnumerable<long> ResolveRows(int column, IEnumerable<long> rows, ITable ancestor) {
			if (ancestor != this)
				throw new Exception("Method routed to incorrect table ancestor.");

			return rows;
		}

		protected override RawTableInfo GetRawTableInfo(RawTableInfo rootInfo) {
			var rows = this.Select(row => row.Id.Number).ToBigArray();
			rootInfo.Add(this, rows);
			return rootInfo;
		}
	}
}