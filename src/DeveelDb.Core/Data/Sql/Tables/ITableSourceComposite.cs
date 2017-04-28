using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Tables {
	public interface ITableSourceComposite {
		Task<ITableSource> CreateTableSourceAsync(TableInfo tableInfo);

		Task<ITableSource> GetTableSourceAsync(int tableId);
	}
}