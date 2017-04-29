using System;
using System.Threading.Tasks;

using Deveel.Data.Indexes;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	public interface ITableSource : IDisposable {
		int TableId { get; }

		TableInfo TableInfo { get; }

		TableEventHistory EventHistory { get; }


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