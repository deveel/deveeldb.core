using System;

namespace Deveel.Data {
	public sealed class SystemEnvironment : ISystemEnvironment {
		public string EnvironmentName { get; set; }

		public string ApplicationName { get; set; }

		public string RootPath { get; set; }
	}
}