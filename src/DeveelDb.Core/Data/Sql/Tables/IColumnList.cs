using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public interface IColumnList : IList<ColumnInfo> {
		int IndexOf(ObjectName columnName);

		int IndexOf(string columnName);

		ObjectName GetColumnName(int offset);
	}
}