using System;

using Deveel.Data.Configuration;

namespace Deveel.Data {
	public sealed class SystemBuildContext {
		public ISystemEnvironment Environment { get; set; }

		public IConfigurationBuilder Settings { get; set; }
	}
}