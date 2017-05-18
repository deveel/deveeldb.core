using System;
using System.IO;

using Xunit;

namespace Deveel.Data.Storage {
	public class InMemoryStoreTests : IDisposable {
		private IStoreSystem storeSystem;

		public InMemoryStoreTests() {
			storeSystem = new InMemoryStoreSystem();
		}

		[Fact]
		public async void CreateAndDeleteStore() {
			await storeSystem.LockAsync("lock1");

			var store = await storeSystem.CreateStoreAsync("test", new Configuration.Configuration());

			Assert.NotNull(store);
			Assert.IsType<InMemoryStore>(store);

			Assert.True(await storeSystem.StoreExistsAsync("test", new Configuration.Configuration()));

			Assert.True(await storeSystem.DeleteStoreAsync(store));
			Assert.False(await storeSystem.StoreExistsAsync("test", new Configuration.Configuration()));

			await Assert.ThrowsAsync<IOException>(() => storeSystem.OpenStoreAsync("test", new Configuration.Configuration()));

			await storeSystem.UnlockAsync("lock1");
		}

		[Fact]
		public async void CreateStoreAndWriteData() {
			await storeSystem.LockAsync("lock1");

			var store = await storeSystem.CreateStoreAsync("test", new Configuration.Configuration());

			var area = store.CreateArea(1024);
			Assert.NotNull(area);
			Assert.Equal(1024, area.Length);

			await area.WriteAsync(12);
			await area.WriteAsync(2L);
			await area.WriteAsync((byte) 22);

			await area.FlushAsync();

			Assert.Equal(13, area.Position);

			Assert.True(await storeSystem.CloseStoreAsync(store));

			await storeSystem.UnlockAsync("lock1");
		}

		public void Dispose() {
			storeSystem.Dispose();
		}
	}
}