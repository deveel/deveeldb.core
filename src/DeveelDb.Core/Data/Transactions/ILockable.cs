using System;

namespace Deveel.Data.Transactions {
	public interface ILockable {
		object RefId { get; }

		void Locked(Lock @lock);

		void Unlocked(Lock @lock);
	}
}