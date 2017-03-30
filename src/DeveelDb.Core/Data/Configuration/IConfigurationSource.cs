using System;
using System.IO;

namespace Deveel.Data.Configuration {
	/// <summary>
	/// A source for stored configurations or destination
	/// to configurations to store.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The streams returned from implementations of this interface
	/// will be passed to <see cref="IConfigurationFormatter"/>
	/// instances for constructing a <see cref="IConfiguration"/> object,
	/// or to store the configurations of a <see cref="IConfiguration"/>
	/// into a given output.
	/// </para>
	/// </remarks>
	public interface IConfigurationSource {
		/// <summary>
		/// Gets a <see cref="Stream"/> that is used to load the
		/// configurations.
		/// </summary>
		Stream InputStream { get; }

		/// <summary>
		/// Gets a <see cref="Stream"/> that can be writtern with
		/// the configurations from a <see cref="IConfiguration"/>.
		/// </summary>
		Stream OutputStream { get; }
	}
}