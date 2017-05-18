using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Deveel.Data.Configuration;

namespace Deveel.Data.Storage {
	public class InMemoryStoreSystem : IStoreSystem {
		private Dictionary<string, InMemoryStore> nameStoreMap;

		public InMemoryStoreSystem() {
			nameStoreMap = new Dictionary<string, InMemoryStore>();
		}

		public string SystemId => "memory";

		public Task<bool> StoreExistsAsync(string name, IConfiguration configuration) {
			lock (this) {
				return Task.FromResult(nameStoreMap.ContainsKey(name));
			}
		}

		async Task<IStore> IStoreSystem.CreateStoreAsync(string name, IConfiguration configuration) {
			return await CreateStoreAsync(name, configuration);
		}

		public Task<InMemoryStore> CreateStoreAsync(string name, IConfiguration configuration) {
			var hashSize = configuration.GetInt32("hashSize", 1024);

			lock (this) {
				if (nameStoreMap.ContainsKey(storeName))
					throw new IOException($"A store named '{storeName}' already in the systme");

				var store = new InMemoryStore(storeName, hashSize);
				nameStoreMap[storeName] = store;
				return Task.FromResult(store);
			}
		}

		async Task<IStore> IStoreSystem.OpenStoreAsync(string name, IConfiguration configuration) {
			return await OpenStoreAsync(name, configuration);
		}

		public Task<InMemoryStore> OpenStoreAsync(string name, IConfiguration configuration) {
			lock (this) {
				InMemoryStore store;
				if (!nameStoreMap.TryGetValue(storeName, out store))
					throw new IOException($"No store with name '{storeName}' was found in the system");

				return Task.FromResult(store);
			}
		}

		Task<bool> IStoreSystem.CloseStoreAsync(IStore store) {
			return CloseStoreAsync((InMemoryStore) store);
		}

		public Task<bool> CloseStoreAsync(InMemoryStore store) {
			lock (this) {
				if (!nameStoreMap.ContainsKey(store.Name))
					throw new IOException($"The store '{store.Name}' was not found in the system");

				return Task.FromResult(true);
			}
		}

		Task<bool> IStoreSystem.DeleteStoreAsync(IStore store) {
			return DeleteStoreAsync((InMemoryStore) store);
		}

		public Task<bool> DeleteStoreAsync(InMemoryStore store) {
			lock (this) {
				try {
					return Task.FromResult(nameStoreMap.Remove(store.Name));
				} finally {
					store.Dispose();
				}
			}
		}

		public Task SetCheckPointAsync() {
			return Task.CompletedTask;
		}

		public Task LockAsync(string lockKey) {
			return Task.CompletedTask;
		}

		public Task UnlockAsync(string lockKey) {
			return Task.CompletedTask;
		}

		private void Clean() {
			lock (this) {
				if (nameStoreMap != null) {
					foreach (var store in nameStoreMap.Values) {
						if (store != null)
							store.Dispose();
					}

					nameStoreMap.Clear();
				}
			}
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				Clean();
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}