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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Deveel {
	public sealed class BigArray<T> : IEnumerable<T> {
		private T[][] items;
		private long blockSize;

		public BigArray(long length) {
			Allocate(length);
		}

		public long Length { get; private set; }

		public T this[long index] {
			get => items[(int)(index / blockSize)][index % blockSize];

			set => items[(int)(index / blockSize)][index % blockSize] = value;
		}

		private void Allocate(long length) {
			if (length < 0)
				throw new ArgumentException("Must specify a length >= 0");

			Length = length;

			if (typeof(T).GetTypeInfo().IsValueType) {
				int itemSize = Marshal.SizeOf<T>();
				blockSize = (int.MaxValue - 56) / itemSize;
			} else {
				int itemSize = IntPtr.Size;
				blockSize = ((int.MaxValue - 56) / itemSize) - 1;
			}

			int blockCount = (int)(length / blockSize);
			if (length > (blockCount * blockSize))
				blockCount++;

			items = new T[blockCount][];

			for (int i = 0; i < blockCount - 1; i++)
				items[i] = new T[blockSize];

			if (blockCount > 0) {
				items[blockCount - 1] = new T[length - ((blockCount - 1) * blockSize)];
			}
		}

		public long IndexOf(T item) {
			return this.IndexOf(item, 0, this.Length);
		}

		public long IndexOf(T item, long startIndex) {
			return this.IndexOf(item, startIndex, this.Length - startIndex);
		}

		public long IndexOf(T item, long startIndex, long count) {
			long index = -1;

			if ((startIndex < 0) || (startIndex > this.Length)) {
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}
			if ((count < 0) || (count > (this.Length - startIndex))) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			long blockIndex = startIndex / blockSize;
			int start = (int)(startIndex % blockSize);
			count += startIndex;
			for (long i = startIndex; i < count && blockIndex < items.Length; blockIndex++) {
				int len = items[blockIndex].Length;

				if (i + len > count) {
					len = (int)(count - i);
				}

				index = Array.IndexOf<T>(items[blockIndex], item, start, len);
				start = 0;
				if (index != -1) {
					index += (blockIndex * blockSize);
					break;
				}

				i += len;
			}

			return index;
		}

		public void CopyTo(long index, T[] destinationArray, long count) {
			this.CopyTo(index, destinationArray, 0, count);
		}

		public void CopyTo(long index, T[] destinationArray, int destinationIndex, long count) {
			if (destinationArray == null) {
				throw new ArgumentNullException(nameof(destinationArray));
			}
			if ((index < 0) || (index > this.Length)) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			if ((destinationIndex < 0) || (destinationIndex > destinationArray.Length)) {
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));
			}
			if ((count < 0) || (count > (this.Length - index)) || (count > (destinationArray.Length - destinationIndex))) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			int destIndex = destinationIndex;
			for (long i = index; i < index + count; i++)
				destinationArray[destIndex++] = this[i];
		}

		public IEnumerator<T> GetEnumerator() {
			return items.SelectMany(x => x).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public static void Copy(BigArray<T> source, long sourceIndex, BigArray<T> target, long targetIndex, long length) {
			if (sourceIndex >= source.Length)
				throw new ArgumentOutOfRangeException(nameof(sourceIndex));
			if (targetIndex + length > target.Length)
				throw new ArgumentException();

			long offset = 0;
			for (long i = sourceIndex; i < length; i++) {
				target[targetIndex + offset++] = source[i];
			}
		}
	}
}