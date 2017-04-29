using System;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	interface ITableSourceResolver {
		ITableSource GetTableSource(int tableId);
	}
}