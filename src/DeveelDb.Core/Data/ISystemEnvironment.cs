using System;

namespace Deveel.Data {
	public interface ISystemEnvironment {
		string EnvironmentName { get; }

		string ApplicationName { get; }

		string RootPath { get; }
	}
}