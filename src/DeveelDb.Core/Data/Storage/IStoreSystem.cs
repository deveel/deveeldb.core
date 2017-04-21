using System;
using System.Threading.Tasks;

using Deveel.Data.Configuration;

namespace Deveel.Data.Storage {
	public interface IStoreSystem : IDisposable {
		Task<bool> StoreExistsAsync(IConfiguration configuration);

		Task<IStore> CreateStoreAsync(IConfiguration configuration);

		Task<IStore> OpenStoreAsync(IConfiguration configuration);

		Task<bool> CloseStoreAsync(IStore store);

		Task<bool> DeleteStoreAsync(IStore store);

		Task SetCheckPointAsync();

		Task LockAsync(string lockKey);

		Task UnlockAsync(string lockKey);
	}
}