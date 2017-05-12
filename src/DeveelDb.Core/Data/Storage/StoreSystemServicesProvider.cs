using System;

using Deveel.Data.Services;

namespace Deveel.Data.Storage {
	class StoreSystemServicesProvider : ISystemServicesProvider {
		public void Register(IScope systemScope) {
			systemScope.Register<IStoreSystem, InMemoryStoreSystem>(KnownStoreSystemNames.InMemory);
			systemScope.Register<IStoreSystem, InMemoryStoreSystem>();
		}
	}
}