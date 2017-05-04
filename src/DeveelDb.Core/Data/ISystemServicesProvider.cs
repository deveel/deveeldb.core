using System;

using Deveel.Data.Services;

namespace Deveel.Data {
	public interface ISystemServicesProvider {
		void Register(IScope systemScope);
	}
}