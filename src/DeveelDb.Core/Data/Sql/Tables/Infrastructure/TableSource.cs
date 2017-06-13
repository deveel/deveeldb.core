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
using System.Text;
using System.Threading.Tasks;

using Deveel.Data.Indexes;
using Deveel.Data.Storage;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	class TableSource : ITableSource {
		private TableSourceComposite composite;
		private TableStateInfo stateInfo;

		private IStoreSystem storeSystem;
		private IStore tableStore;

		private FixedRecordList recordList;
		private long listHeaderOffset;

		public TableSource(TableSourceComposite composite, TableStateInfo stateInfo) {
			this.composite = composite;
			this.stateInfo = stateInfo;

			storeSystem = composite.ResolveStoreSystem(stateInfo.SystemId);

			// Generate the name of the store file name.
			StoreIdentity = MakeSourceIdentity();
		}

		~TableSource() {
			Dispose(false);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (tableStore != null) {
					storeSystem.CloseStoreAsync(tableStore).Wait();
					tableStore.Dispose();
				}
			}

			storeSystem = null;
			tableStore = null;
			composite = null;
		}

		public int TableId => stateInfo.Id;

		public TableInfo TableInfo { get; private set; }

		public string StoreIdentity { get; }

		private string MakeSourceIdentity() {
			string str = stateInfo.SourceName.Replace('.', '_').ToLower();

			// Go through each character and remove each non a-z,A-Z,0-9,_ character.
			// This ensure there are no strange characters in the file name that the
			// underlying OS may not like.
			var osifiedName = new StringBuilder();
			int count = 0;
			for (int i = 0; i < str.Length || count > 64; ++i) {
				char c = str[i];
				if ((c >= 'a' && c <= 'z') ||
				    (c >= 'A' && c <= 'Z') ||
				    (c >= '0' && c <= '9') ||
				    c == '_') {
					osifiedName.Append(c);
					++count;
				}
			}

			return $"{stateInfo.Id}_{osifiedName}";
		}

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

		public void Open() {
			var broken = OpenTable();

			if (broken) {
				
			}
		}

		private bool OpenTable() {
			// TODO: table-specific configuration 
			// Open the store.
			tableStore = storeSystem.OpenStoreAsync(StoreIdentity, new Configuration.Configuration()).Result;

			bool broken = tableStore.State == StoreState.Broken;

			// Setup the list structure
			recordList = new FixedRecordList(tableStore, 12);

			// Read and setup the pointers
			ReadStoreHeaders();

			return broken;
		}

		private void ReadStoreHeaders() {
			throw new NotImplementedException();
		}

		public bool Exists() {
			return storeSystem.StoreExistsAsync(StoreIdentity).Result;
		}
	}
}