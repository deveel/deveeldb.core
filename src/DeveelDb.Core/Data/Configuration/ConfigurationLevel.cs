using System;

namespace Deveel.Data.Configuration {
	/// <summary>
	/// Defines the level of configuration settings to save or read
	/// </summary>
	public enum ConfigurationLevel {
		/// <summary>
		/// Gets or stores only the current level of configuration.
		/// </summary>
		Current = 1,

		/// <summary>
		/// Includes all the configuration tree.
		/// </summary>
		Deep = 2
	}
}
