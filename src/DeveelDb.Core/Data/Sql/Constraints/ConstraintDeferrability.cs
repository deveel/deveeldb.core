using System;

namespace Deveel.Data.Sql.Constraints {
	/// <summary>
	/// The type of deferrance of a constraint.
	/// </summary>
	public enum ConstraintDeferrability : short {
		/// <summary>
		/// The constraint is checked at the <c>COMMIT</c>
		/// of each transaction.
		/// </summary>
		InitiallyDeferred = 4,

		/// <summary>
		/// The constraint is checked immediately after
		/// each single statement.
		/// </summary>
		InitiallyImmediate = 5,

		/// <summary>
		/// A constraint whose check cannot be deferred to the
		/// <c>COMMIT</c> of a trasnaction.
		/// </summary>
		NotDeferrable = 6,
	}
}