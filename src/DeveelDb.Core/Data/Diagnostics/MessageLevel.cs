using System;

namespace Deveel.Data.Diagnostics {
	/// <summary>
	/// The declaration of the level of importance and
	/// severity of messages output from the system
	/// </summary>
	public enum MessageLevel {
		/// <summary>
		/// Denotes a message that has the aim to trace
		/// execution of parts of the application
		/// </summary>
		Trace = 1,

		/// <summary>
		/// Detailed diagnostic information to administrators on
		/// the execution of parts of the system
		/// </summary>
		Debug = 2,

		/// <summary>
		/// Marks a message that carries operation information
		/// of higher level
		/// </summary>
		Information = 3,

		/// <summary>
		/// Represents an oddity in the system, not serious
		/// enough to represent an error
		/// </summary>
		Warning = 4,

		/// <summary>
		/// An error in the execution of an operation that
		/// determines the failure of the single operation
		/// </summary>
		Error = 5,

		/// <summary>
		/// An error that causes the interruption of the system
		/// and the termination of all the operations
		/// </summary>
		Fatal = 6
	}
}