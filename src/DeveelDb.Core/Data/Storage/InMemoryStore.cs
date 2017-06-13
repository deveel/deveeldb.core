﻿// 
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
using System.IO;
using System.Threading.Tasks;

namespace Deveel.Data.Storage {
	public class InMemoryStore : IStore {
		private InMemoryBlock fixedAreaBlock;
		private InMemoryBlock[] areaMap;
		private long uniqueIdKey;

		internal InMemoryStore(string name, int hashSize) {
			Name = name;
			areaMap = new InMemoryBlock[hashSize];
			uniqueIdKey = 0;
		}

		~InMemoryStore() {
			Dispose(false);
		}

		/// <summary>
		/// Gets the unique name of the store within the application.
		/// </summary>
		public string Name { get; }


		StoreState IStore.State => StoreState.Normal;

		private InMemoryBlock FixedAreaBlock {
			get {
				lock (this) {
					if (fixedAreaBlock == null)
						fixedAreaBlock = new InMemoryBlock(-1, 64);

					return fixedAreaBlock;
				}
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				if (areaMap != null) {
					for (int i = 0; i < areaMap.Length; i++) {
						var block = areaMap[i];
						if (block != null)
							block.Dispose();

						areaMap[i] = null;
					}
				}
			}

			fixedAreaBlock = null;
			areaMap = null;
		}

		private InMemoryBlock GetBlock(long pointer) {
			if (pointer == -1)
				return FixedAreaBlock;

			return GetAreaBlock(pointer);
		}

		private InMemoryBlock GetAreaBlock(long pointer) {
			lock (this) {
				// Find the pointer in the hash
				var hashPos = (int)(pointer % areaMap.Length);
				InMemoryBlock prev = null;
				var block = areaMap[hashPos];

				// Search for this pointer
				while (block != null && block.Id != pointer) {
					prev = block;
					block = block.Next;
				}

				if (block == null)
					throw new IOException("Pointer " + pointer + " is invalid.");

				// Move the element to the start of the list.
				if (prev != null) {
					prev.Next = block.Next;
					block.Next = areaMap[hashPos];
					areaMap[hashPos] = block;
				}

				return block;
			}
		}

		/// <inheritdoc/>
		public IArea CreateArea(long size) {
			if (size > Int32.MaxValue)
				throw new IOException("'size' is too large.");

			lock (this) {
				// Generate a unique id for this area.
				long id = uniqueIdKey;
				++uniqueIdKey;

				// Create the element.
				var element = new InMemoryBlock(id, (int)size);

				// The position in the hash map
				int hashPos = (int)(id % areaMap.Length);

				// Add to the chain
				element.Next = areaMap[hashPos];
				areaMap[hashPos] = element;

				return element.GetArea(false);
			}
		}

		/// <inheritdoc/>
		public void DeleteArea(long id) {
			lock (this) {
				// Find the pointer in the hash
				var hashPos = (int)(id % areaMap.Length);
				InMemoryBlock prev = null;
				InMemoryBlock block = areaMap[hashPos];

				// Search for this pointer
				while (block != null && block.Id != id) {
					prev = block;
					block = block.Next;
				}

				// If not found
				if (block == null)
					throw new IOException("Area ID " + id + " is invalid.");

				// Remove
				if (prev == null) {
					areaMap[hashPos] = block.Next;
				} else {
					prev.Next = block.Next;
				}

				// Garbage collector should do the rest...
			}
		}

		/// <inheritdoc/>
		public IArea GetArea(long id, bool readOnly) {
			return GetBlock(id).GetArea(readOnly);
		}

		/// <inheritdoc/>
		public void Lock() {
		}

		/// <inheritdoc/>
		public void Unlock() {
		}

		#region InMemoryBlock

		class InMemoryBlock : IDisposable {
			private byte[] block;

			public InMemoryBlock(long id, int size) {
				Id = id;
				block = new byte[size];
			}

			~InMemoryBlock() {
				Dispose(false);
			}

			public long Id { get; private set; }

			public InMemoryBlock Next { get; set; }

			public void Dispose() {
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing) {
				if (disposing) {
					if (block != null)
						Array.Resize(ref block, 0);
				}

				block = null;
			}

			public IArea GetArea(bool readOnly) {
				return new InMemoryArea(Id, readOnly, block, 0, block.Length);
			}
		}

		#endregion

		#region InMemoryArea

		class InMemoryArea : IArea {
			private byte[] data;
			private int position;
			private readonly int startPosition;
			private readonly int endPosition;

			public InMemoryArea(long id, bool readOnly, byte[] data, int offset, int length) {
				this.data = data;
				Length = length;

				position = startPosition = offset;
				endPosition = offset + length;

				Id = id;
				IsReadOnly = readOnly;
			}

			~InMemoryArea() {
				Dispose(false);
			}

			public long Id { get; private set; }

			public bool IsReadOnly { get; private set; }

			public int Position {
				get { return position; }
				set {
					var actPosition = startPosition + value;
					if (actPosition < 0 || actPosition >= endPosition)
						throw new IOException("Moved position out of bounds.");

					position = actPosition;
				}
			}

			public int Capacity {
				get { return endPosition - startPosition; }
			}

			public int Length { get; }

			private int CheckPositionBounds(int diff) {
				var newPos = position + diff;
				if (newPos > endPosition)
					throw new IOException(String.Format("Attempt to read out of bounds: from {0} to {1} (position {2} to {3})",
						startPosition, endPosition, position, newPos));

				var oldPos = position;
				position = newPos;
				return oldPos;
			}

			private void Dispose(bool disposing) {
				if (disposing) {
					if (data != null)
						Array.Resize(ref data, 0);
				}

				data = null;
			}

			public void Dispose() {
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			public void CopyTo(IArea destArea, int size) {
				const int bufferSize = 2048;
				byte[] buf = new byte[bufferSize];
				int toCopy = System.Math.Min(size, bufferSize);

				while (toCopy > 0) {
					var read = Read(buf, 0, toCopy);
					if (read == 0)
						break;

					destArea.Write(buf, 0, toCopy);
					size -= toCopy;
					toCopy = System.Math.Min(size, bufferSize);
				}
			}

			public int Read(byte[] buffer, int offset, int length) {
				Array.Copy(data, CheckPositionBounds(length), buffer, offset, length);
				return length;
			}

			public void Write(byte[] buffer, int offset, int length) {
				Array.Copy(buffer, offset, data, CheckPositionBounds(length), length);
			}

			public void Flush() {
			}
		}

		#endregion

	}
}