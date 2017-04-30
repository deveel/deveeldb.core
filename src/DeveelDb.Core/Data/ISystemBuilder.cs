using System;

using Deveel.Data.Services;

namespace Deveel.Data {
	public interface ISystemBuilder {
		IDatabaseSystem Build();

		ISystemBuilder UseSetting(string key, object value);

		ISystemBuilder UseScope(Func<IScope> scope);

		ISystemBuilder ConfigureServices(Action<SystemBuildContext, IScope> configure);
	}
}
