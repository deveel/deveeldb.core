using System;

namespace Deveel.Data.Configuration {
	/// <summary>
	/// Defines an interface to access a configuration handled
	/// by a given object.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Typical configuration providers are databases and systems.
	/// </para>
	/// </remarks>
	public interface IConfigurationProvider {
		/// <summary>
		/// Gets the object specific configuration.
		/// </summary>
		IConfiguration Configuration { get; }
	}
}
