using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public interface ITable : IDbObject, ISqlValue, IEnumerator<Row> {
		long RowCount { get; }


		SqlObject GetValue(long row, int column);
	}
}