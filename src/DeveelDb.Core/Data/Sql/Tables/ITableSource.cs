using System;
using System.Threading.Tasks;

using Deveel.Data.Indexes;
using Deveel.Data.Transactions;

namespace Deveel.Data.Sql.Tables {
	public interface ITableSource : IDisposable {
		int TableId { get; }

		TableInfo TableInfo { get; }


		Task<long> GetCurrentUniqueIdAsync();

		Task SetUniqueIdAsync(long value);

		Task<long> GetNextUniqueIdAsync();

		Task<IIndexSet<SqlObject,long>> CreateIndexSetAsync();


		Task<IMutableTable> CreateTableAsync(IContext context);

		Task<long> AddRowAsync(Row row);

		Task<RecordState> WriteRecordStateAsync(long rowNumber, RecordState recordState);

		Task BuildIndexesAsync();
	}
}