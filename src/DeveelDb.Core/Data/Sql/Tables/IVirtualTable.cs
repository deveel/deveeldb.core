using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public interface IVirtualTable : ITable {
		IEnumerable<long> ResolveRows(int column, IEnumerable<long> rowSet, ITable ancestor);

		RawTableInfo GetRawTableInfo(RawTableInfo rootInfo);
	}
}