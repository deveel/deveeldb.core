using System.Threading.Tasks;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public interface ITableSourceComposite {
		Task<ITableSource> CreateTableSourceAsync(TableInfo tableInfo);

		Task<ITableSource> GetTableSourceAsync(int tableId);
	}
}