using System;
using System.Collections.Generic;

namespace Deveel.Data.Transactions {
	public class Locker : IDisposable {
		private List<LockHandle> openHandles;
		private Dictionary<object, LockingQueue> queuesMap;
		internal IContext context;

		public Locker(IContext context) {
			this.context = context;
			openHandles = new List<LockHandle>(128);
			queuesMap = new Dictionary<object, LockingQueue>();
		}

		~Locker() {
			Dispose(false);
		}

		private LockingQueue GetQueueFor(ILockable lockable) {
			LockingQueue queue;

			if (!queuesMap.TryGetValue(lockable.RefId, out queue)) {
				queue = new LockingQueue(context, lockable);
				queuesMap[lockable.RefId] = queue;
			}

			return queue;
		}

		public LockHandle Lock(ILockable lockable, AccessType accessType, LockingMode mode)
			=> Lock(new[] {lockable}, accessType, mode);

		public LockHandle Lock(ILockable[] lockables, AccessType accessType, LockingMode mode) {
			lock (this) {
				var handle = new LockHandle(this);

				for (int i = lockables.Length - 1; i >= 0; --i) {
					var lockable = lockables[i];
					var queue = GetQueueFor(lockable);

					if ((accessType & AccessType.Read) != 0)
						handle.AddLock(queue.Lock(mode, AccessType.Read));
					if ((accessType & AccessType.Write) != 0)
						handle.AddLock(queue.Lock(mode, AccessType.Write));
				}

				openHandles.Add(handle);

				return handle;
			}
		}

		internal void Release(LockHandle handle) {
			lock (this) {
				if (openHandles != null) {
					var index = openHandles.IndexOf(handle);
					if (index >= 0)
						openHandles.RemoveAt(index);
				}

				handle.ReleaseLocks();
			}
		}

		public bool IsLocked(ILockable lockable) {
			lock (this) {
				LockingQueue queue;
				if (!queuesMap.TryGetValue(lockable.RefId, out queue))
					return false;

				return !queue.IsEmpty;
			}
		}

		public void Wait(ILockable[] lockables, AccessType accessType, int timeout) {
			if (openHandles == null || lockables == null)
				return;

			foreach (var handle in openHandles) {
				foreach (var lockable in lockables) {
					if (handle.IsHandled(lockable))
						handle.Wait(lockable, accessType, timeout);
				}
			}
		}

		private void ReleaseAll() {
			lock (this) {
				if (openHandles != null) {
					foreach (var handle in openHandles) {
						handle.ReleaseLocks();
					}

					openHandles.Clear();
				}
			}
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				ReleaseAll();
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}