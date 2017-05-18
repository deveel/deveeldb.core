// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


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

		protected void Publish(TMessage message) {
			lock (((ICollection)queue).SyncRoot) {
				queue.Enqueue(message);
			}

			semaphore.Set();
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (semaphore != null)
					semaphore.Dispose();

				if (tokenSource != null &&
				    !tokenSource.IsCancellationRequested) {

					try {
						tokenSource.Cancel(true);
						Task.WaitAll(consumeTasks, tokenSource.Token);
					} catch (OperationCanceledException) {
					}
				}

				if (tokenSource != null)
					tokenSource.Dispose();

				if (queue != null) {
					lock (((ICollection)queue).SyncRoot) {
						queue.Clear();
					}
				}
			}

			semaphore = null;
			tokenSource = null;
			queue = null;
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