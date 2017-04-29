using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Deveel.Data.Diagnostics {
	public abstract class ThreadedQueue<TMessage> : IDisposable {
		private CancellationTokenSource tokenSource;
		private AutoResetEvent semaphore;
		private Task[] consumeTasks;
		private Queue<TMessage> queue;

		protected ThreadedQueue(int threadCount) {
			queue = new Queue<TMessage>(1024);
			semaphore = new AutoResetEvent(false);

			tokenSource = new CancellationTokenSource();

			consumeTasks = new Task[threadCount];

			for (int i = 0; i < threadCount; i++) {
				consumeTasks[i] = Task.Run(() => Consume(), tokenSource.Token);
			}
		}

		public void Publish(TMessage message) {
			lock (((ICollection)queue).SyncRoot) {
				queue.Enqueue(message);
			}

			semaphore.Set();
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (tokenSource != null &&
					!tokenSource.IsCancellationRequested)
					tokenSource.Cancel(false);

				Task.WaitAll(consumeTasks);

				if (tokenSource != null)
					tokenSource.Dispose();

				if (semaphore != null)
					semaphore.Dispose();
			}

			semaphore = null;
			tokenSource = null;
		}

		private void Consume() {
			semaphore.WaitOne();

			TMessage message;

			lock (((ICollection)queue).SyncRoot) {
				message = queue.Dequeue();
			}

			Consume(message);
		}

		protected abstract void Consume(TMessage message);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}