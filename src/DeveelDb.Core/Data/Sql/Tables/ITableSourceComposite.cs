using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Tables {
	public interface ITableSourceComposite : IDisposable {
		Task<ITableSource> CreateTableSourceAsync(TableInfo tableInfo);

		Task<ITableSource> GetTableSourceAsync(int tableId);
	}
}