// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


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