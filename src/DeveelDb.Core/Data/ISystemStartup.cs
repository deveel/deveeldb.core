using System;

using Deveel.Data.Services;

namespace Deveel.Data {
	public interface ISystemStartup {
		void ConfigureServices(IScope scope);
	}
}