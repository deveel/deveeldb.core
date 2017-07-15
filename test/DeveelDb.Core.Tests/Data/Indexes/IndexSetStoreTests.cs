using System;

using Deveel.Data.Storage;

using Xunit;

namespace Deveel.Data.Indexes {
	public class IndexSetStoreTests : IDisposable {
		private IStore store;
		private IndexSetStore indexSetStore;

		public IndexSetStoreTests() {
			var storeSystem = new InMemoryStoreSystem();
			store = storeSystem.CreateStoreAsync("idx", new Configuration.Configuration()).Result;

			indexSetStore = new IndexSetStore(store);
		}

		public void Dispose() {
			indexSetStore.Dispose();
			store.Dispose();
		}

		[Fact]
		public void CreateOpenAndClose() {
			var offset = indexSetStore.Create();
			Assert.True(offset >= 0);

			indexSetStore.Open(offset);
			indexSetStore.Close();
		}

		[Fact]
		public void CommitIndexSet() {
			var offset = indexSetStore.Create();
			indexSetStore.Open(offset);

			indexSetStore.PrepareIndexes(1, 1, 1024);

			var testIndexSet = indexSetStore.GetSnapshotIndex();
			var index = testIndexSet.GetIndex(0);

			Assert.NotNull(index);

			index.Add(93044);

			indexSetStore.CommitIndexSet(testIndexSet);
		}

		[Fact]
		public void CommitDropIndex() {
			var offset = indexSetStore.Create();
			indexSetStore.Open(offset);

			indexSetStore.PrepareIndexes(1, 1, 1024);

			indexSetStore.CommitDropIndex(0);
		}
	}
}