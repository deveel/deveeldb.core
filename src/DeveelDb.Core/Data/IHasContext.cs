using System;

namespace Deveel.Data {
	/// <summary>
	/// Marks an element of the system as handler of a context
	/// </summary>
	/// <remarks>
	/// Implementations of this contract provide access to a
	/// <see cref="IContext"/> object, that handles the contextual
	/// references to configurations and services.
	/// </remarks>
	/// <seealso cref="IContext"/>
	public interface IHasContext {
		/// <summary>
		/// Gets the context handled by the object.
		/// </summary>
		IContext Context { get; }
	}
}
