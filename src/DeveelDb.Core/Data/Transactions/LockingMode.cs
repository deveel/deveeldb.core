using System;

namespace Deveel.Data.Transactions {
	/// <summary>
	/// The mode applied to a lock over a resource during
	/// a transaction.
	/// </summary>
	public enum LockingMode {
		/// <summary>
		/// No specific locking was applied to the resource
		/// </summary>
		None = 0,

		/// <summary>
		/// An <c>exclusive</c> lock was applied to the resource: this
		/// can be accessed only by the transaction that owns the lock
		/// or by child transactions.
		/// </summary>
		Exclusive = 1,

		/// <summary>
		/// A lock that is <c>shared</c> between transactions, allowing
		/// read of data on locked objects, but preventing any write.
		/// </summary>
		Shared = 2
	}
}