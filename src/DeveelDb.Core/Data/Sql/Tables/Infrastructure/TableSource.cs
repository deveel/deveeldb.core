using System;
using System.Threading.Tasks;

using Deveel.Data.Indexes;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	class TableSource : ITableSource {
		public void Dispose() {
			throw new NotImplementedException();
		}

		public int TableId { get; }

		public TableInfo TableInfo { get; }

		public async Task<long> GetCurrentUniqueIdAsync() {
			throw new NotImplementedException();
		}

		public async Task SetUniqueIdAsync(long value) {
			throw new NotImplementedException();
		}

		public async Task<long> GetNextUniqueIdAsync() {
			throw new NotImplementedException();
		}

		public async Task<IIndexSet<SqlObject, long>> CreateIndexSetAsync() {
			throw new NotImplementedException();
		}

		public async Task<IMutableTable> CreateTableAsync(IContext context) {
			throw new NotImplementedException();
		}

		public async Task<long> AddRowAsync(Row row) {
			throw new NotImplementedException();
		}

		public async Task<RecordState> WriteRecordStateAsync(long rowNumber, RecordState recordState) {
			throw new NotImplementedException();
		}

		public async Task BuildIndexesAsync() {
			throw new NotImplementedException();
		}
	}
}