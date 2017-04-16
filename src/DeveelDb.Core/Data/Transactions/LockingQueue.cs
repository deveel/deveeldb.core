using System;
using System.Collections.Generic;
using System.Threading;

using Deveel.Data.Configuration;

namespace Deveel.Data.Transactions {
	public sealed class LockingQueue {
		private readonly IContext context;
		private readonly List<Lock> locks;

		public LockingQueue(IContext context, ILockable locked) {
			this.context = context;
			Locked = locked;
			locks = new List<Lock>();
		}

		internal ILockable Locked { get; }

		internal bool IsEmpty => locks.Count == 0;

		internal Lock Lock(LockingMode mode, AccessType accessType) {
			lock (this) {
				var @lock = new Lock(this, mode, accessType);
				Locked.Locked(@lock);
				locks.Add(@lock);
				@lock.Acquire();

				return @lock;
			}
		}

		internal void Release(Lock @lock) {
			lock (this) {
				locks.Remove(@lock);
				Locked.Unlocked(@lock);
				Monitor.Pulse(this);
			}
		}

		private LockTimeoutException TimeoutException(AccessType accessType, int timeout) {
			ObjectName tableName;
			if (Locked is IDbObject) {
				tableName = ((IDbObject)Locked).ObjectInfo.FullName;
			} else {
				tableName = new ObjectName(Locked.RefId.ToString());
			}

			return new LockTimeoutException(tableName, accessType, timeout);
		}


		internal void Wait(Lock @lock, int timeout) {
			lock (this) {
				// Error checking.  The queue must contain the Lock.
				if (!locks.Contains(@lock))
					throw new InvalidOperationException("Queue does not contain the given Lock");

				// If 'READ'
				bool blocked;
				int index;
				if (@lock.AccessType == AccessType.Read) {
					do {
						blocked = false;

						index = locks.IndexOf(@lock);

						int i;
						for (i = index - 1; i >= 0 && !blocked; --i) {
							var testLock = locks[i];
							if (testLock.AccessType == AccessType.Write)
								blocked = true;
						}

						if (blocked) {
							if (!Monitor.Wait(this, timeout))
								throw TimeoutException(@lock.AccessType, timeout);
						}
					} while (blocked);
				} else {
					do {
						blocked = false;

						index = locks.IndexOf(@lock);

						if (index != 0) {
							blocked = true;

							if (!Monitor.Wait(this, timeout))
								throw TimeoutException(@lock.AccessType, timeout);
						}

					} while (blocked);
				}

				// Notify the Lock table that we've got a lock on it.
				// TODO: Lock.Table.LockAcquired(Lock);
			}
		}
	}
}