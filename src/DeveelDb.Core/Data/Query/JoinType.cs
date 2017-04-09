using System;

namespace Deveel.Data.Query {
	/// <summary>
	/// Enumerates the kind of group join in a selection query.
	/// </summary>
	public enum JoinType {
		/// <summary>
		/// A join that requires both sources joined to have
		/// matching records.
		/// </summary>
		Inner = 1,

		/// <summary>
		/// Returns all the records in the left side of the join, even if
		/// the other side has no corresponding records.
		/// </summary>
		Left = 2,

		/// <summary>
		/// Returns all the records in the right side of the join, even if
		/// the other side has no corresponding records.
		/// </summary>
		Right = 3,

		/// <summary>
		/// 
		/// </summary>
		Full = 4,

		/// <summary>
		/// Defaults to the natural join between two sources.
		/// </summary>
		None = -1,
	}
}