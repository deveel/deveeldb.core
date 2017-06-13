using System;

using Xunit;

namespace Deveel.Data.Storage {
	public class FixedRecordListTests : IDisposable {
		private IStore store;
		private FixedRecordList recordList;

		public FixedRecordListTests() {
			var storeSystem = new InMemoryStoreSystem();
			store = storeSystem.CreateStoreAsync("test", new Configuration.Configuration()).Result;

			recordList = new FixedRecordList(store, 256);
		}

		[Fact]
		public void CreateNew() {
			var header = recordList.Create();

			Assert.Equal(0, header);
			Assert.Equal(0, recordList.NodeCount);
			Assert.Equal(0, recordList.BlockCount);

			var blockIndex = recordList.IncreaseSize();

			var recordArea = recordList.GetRecord(blockIndex);
			Assert.NotNull(recordArea);
		}

		public void Dispose() {
			recordList.Dispose();
			store.Dispose();
		}
	}
}