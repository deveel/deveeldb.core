using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Configuration;

namespace Deveel.Data.Transactions {
	public sealed class LockHandle : IDisposable {
		private bool released;
		private List<Lock> locks;
		private readonly Locker locker;

		internal LockHandle(Locker locker) {
			this.locker = locker;
			locks = new List<Lock>();
		}

		~LockHandle() {
			Dispose(false);
		}

		internal void AddLock(Lock @lock) {
			locks.Add(@lock);
		}

		internal void ReleaseLocks() {
			if (!released) {
				for (int i = locks.Count - 1; i >= 0; i--) {
					locks[i].Release();
				}

				locks.Clear();
				released = true;
			}
		}

		internal void Wait(ILockable lockable, AccessType accessType, int timeout) {
			bool found = false;
			for (int i = locks.Count - 1; i >= 0; i--) {
				var @lock = locks[i];
				if (@lock.Locked.RefId.Equals(lockable.RefId) &&
				    (@lock.AccessType & accessType) != 0) {
					@lock.Wait(accessType, timeout);
					found = true;
				}
			}

			if (!found)
				throw new InvalidOperationException("The handle does not lock the object trying to access");

		}

		public void WaitAll(int timeout) {
			for (int i = locks.Count - 1; i >= 0; i--) {
				locks[i].Wait(locks[i].AccessType, timeout);
			}
		}

		public void WaitAll() {
			var timeout = locker.context.GetValue<int>("transaction.lock.timeout", 1500);
			WaitAll(timeout);
		}

		public void Release() {
			locker.Release(this);
		}

		public bool IsHandled(ILockable lockable) {
			for (int i = locks.Count - 1; i >= 0; i--) {
				if (locks[i].Locked.RefId.Equals(lockable.RefId))
					return true;
			}

			return false;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (!released)
					Release();
			}
		}
	}
}