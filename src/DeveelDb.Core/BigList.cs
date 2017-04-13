using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Deveel {
	public class BigList<T> : IList<T> {
		private BigArray<T> items;
		private long size;
		private int version;

		public BigList()
			: this(0) {
		}

		public BigList(long capacity) {
			items = new BigArray<T>(capacity);
			size = 0;
		}

		public BigList(IEnumerable<T> collection) {
			ICollection<T> c = collection as ICollection<T>;
			if (c != null) {
				int count = c.Count;
				if (count == 0) {
					items = new BigArray<T>(0);
				} else {
					items = new BigArray<T>(count);
					var array = new T[count];
					c.CopyTo(array, 0);

					for (int i = 0; i < array.Length; i++) {
						items[i] = array[i];
					}

					size = count;
				}
			} else {
				size = 0;
				items = new BigArray<T>(0);
				AddEnumerable(collection);
			}
		}

		public long Capacity {
			get { return items.Length; }
			set {
				if (value != items.Length) {
					if (value > 0) {
						var newItems = new BigArray<T>(value);
						if (size > 0) {
							items.CopyTo(0, newItems, 0, size);
						}
						items = newItems;
					} else {
						items = new BigArray<T>(0);
					}
				}
			}
		}

		private void Allocate(int itemCount) {
			if (size + itemCount > items.Length) {
				var capacity = itemCount + Capacity;
				Capacity = capacity;
			}
		}

		private void AddEnumerable(IEnumerable<T> enumerable) {
			using (IEnumerator<T> en = enumerable.GetEnumerator()) {
				version++;

				while (en.MoveNext()) {
					// Capture Current before doing anything else. If this throws
					// an exception, we want to make a clean break.
					T current = en.Current;

					Allocate(1);

					items[size++] = current;
				}
			}
		}

		public IEnumerator<T> GetEnumerator() {
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item) {
			var array = items;
			var size = this.size;
			version++;
			if ((ulong)size < (ulong)array.Length) {
				this.size = size + 1;
				array[size] = item;
			} else {
				AddWithResize(item);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void AddWithResize(T item) {
			var size = this.size;
			Allocate(1);
			this.size = size + 1;
			items[size] = item;
		}

		public void AddRange(IEnumerable<T> collection) {
			InsertRange(size, collection);
		}

		public void InsertRange(long index, IEnumerable<T> collection) {
			if (collection == null) {
				throw new ArgumentNullException(nameof(collection));
			}

			if ((ulong)index > (ulong)size) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			ICollection<T> c = collection as ICollection<T>;
			if (c != null) {    // if collection is ICollection<T>
				int count = c.Count;
				if (count > 0) {
					Allocate(count);

					if (index < size) {
						items.CopyTo(index, items, index + count, size - index);
					}

					// If we're inserting a List into itself, we want to be able to deal with that.
					if (this == c) {
						// Copy first part of _items to insert location
						items.CopyTo(0, items, index, index);
						// Copy last part of _items back to inserted location
						items.CopyTo(index + count, items, index * 2, size - index);
					} else {
						var array = new T[c.Count];
						c.CopyTo(array, (int)index);
						for (int i = 0; i < array.Length; i++) {
							items[i] = array[i];
						}
					}
					size += count;
				}
			} else if (index < size) {
				// We're inserting a lazy enumerable. Call Insert on each of the constituent items.
				using (IEnumerator<T> en = collection.GetEnumerator()) {
					while (en.MoveNext()) {
						Insert(index++, en.Current);
					}
				}
			} else {
				// We're adding a lazy enumerable because the index is at the end of this list.
				AddEnumerable(collection);
			}
			version++;
		}

		public void Clear() {
			var size = this.size;
			this.size = 0;
			version++;
			if (size > 0) {
				BigArray<T>.Clear(items, 0, size); // Clear the elements so that the gc can reclaim the references.
			}
		}

		public bool Contains(T item) {
			return size != 0 && IndexOf(item) != -1;
		}

		public void CopyTo(T[] array, int arrayIndex) {
			if ((array != null) && (array.Rank != 1))
				throw new ArgumentException();

			try {
				items.CopyTo(0, array, arrayIndex, size);
			} catch (ArrayTypeMismatchException) {
				throw new ArgumentException();
			}
		}

		public bool Remove(T item) {
			var index = IndexOf(item);
			if (index >= 0) {
				RemoveAt(index);
				return true;
			}

			return false;
		}

		int ICollection<T>.Count => (int)Count;

		public long Count => size;

		bool ICollection<T>.IsReadOnly => false;

		int IList<T>.IndexOf(T item) {
			return (int)IndexOf(item);
		}

		public long IndexOf(T item) {
			return items.IndexOf(item, 0, size);
		}

		void IList<T>.Insert(int index, T item) {
			Insert(index, item);
		}

		public void Insert(long index, T item) {
			Allocate(1);

			if (index < size)
				items.CopyTo(index, items, index + 1, size - index);

			items[index] = item;
			size++;
			version++;
		}

		void IList<T>.RemoveAt(int index) {
			RemoveAt(index);
		}

		public void RemoveAt(long index) {
			if ((ulong)index >= (ulong)size)
				throw new ArgumentOutOfRangeException(nameof(index));

			size--;
			if (index < size) {
				items.CopyTo(index + 1, items, index, size - index);
			}
			version++;
		}

		T IList<T>.this[int index] {
			get { return this[index]; }
			set { this[index] = value; }
		}

		public T this[long index] {
			get {
				if (index < 0 || index > size)
					throw new ArgumentOutOfRangeException(nameof(index));

				return items[index];
			}
			set {
				if (index < 0 || index > size)
					throw new ArgumentOutOfRangeException(nameof(index));

				items[index] = value;
				version++;
			}
		}

		#region Enumerator

		class Enumerator : IEnumerator<T> {
			private readonly BigList<T> list;
			private long offset;
			private long count;
			private int version;

			public Enumerator(BigList<T> list) {
				this.list = list;
				Reset();
			}

			private void AssertVersion() {
				if (version != list.version)
					throw new InvalidOperationException();
			}

			public bool MoveNext() {
				AssertVersion();
				return ++offset < count;
			}

			public void Reset() {
				count = list.size;
				offset = -1;
				version = list.version;
			}

			public T Current {
				get {
					AssertVersion();
					return list.items[offset];
				}
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public void Dispose() {
			}
		}

		#endregion
	}
}