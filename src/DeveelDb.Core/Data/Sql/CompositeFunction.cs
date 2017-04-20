using System;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql {
	/// <summary>
	/// The kind of composite function in a <see cref="CompositeTable"/>.
	/// </summary>
	public enum CompositeFunction {
		/// <summary>
		/// The composite function for finding the union of the tables.
		/// </summary>
		Union = 1,

		/// <summary>
		/// The composite function for finding the interestion of the tables.
		/// </summary>
		Intersect = 2,

		/// <summary>
		/// The composite function for finding the difference of the tables.
		/// </summary>
		Except = 3,

		/// <summary>
		/// An unspecified composite function.
		/// </summary>
		None = -1
	}
}