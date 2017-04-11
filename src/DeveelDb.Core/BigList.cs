using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel {
	public class BigList<T> : IList<T> {
		private BigArray<T> items;
		private long size;

		public BigList() 
			: this(1024*4) {
		}

		public BigList(long capacity) {
			items = new BigArray<T>(capacity);
			size = 0;
		}

		public long Capacity => items.Length;

		private void Allocate(int itemCount) {
			if (size + itemCount > items.Length) {
				var newLength = items.Length + Capacity;
				var newArray = new BigArray<T>(newLength);
				BigArray<T>.Copy(items, 0, newArray, 0, size);
				items = newArray;
			}
		}

		public IEnumerator<T> GetEnumerator() {
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Add(T item) {
			throw new NotImplementedException();
		}

		public void Clear() {
			throw new NotImplementedException();
		}

		public bool Contains(T item) {
			throw new NotImplementedException();
		}

		public void CopyTo(T[] array, int arrayIndex) {
			throw new NotImplementedException();
		}

		public bool Remove(T item) {
			throw new NotImplementedException();
		}

		int ICollection<T>.Count => (int) Count;

		public long Count => size;

		public bool IsReadOnly { get; }

		int IList<T>.IndexOf(T item) {
			return (int) IndexOf(item);
		}

		public long IndexOf(T item) {
			throw new NotImplementedException();
		}

		void IList<T>.Insert(int index, T item) {
			Insert(index, item);
		}

		public void Insert(long index, T item) {
			Allocate(1);

			if (index < size)
				BigArray<T>.Copy(items, index, items, index + 1, size - index);

			items[index] = item;
			size++;
		}

		void IList<T>.RemoveAt(int index) {
			RemoveAt(index);
		}

		public void RemoveAt(long index) {
			size--;
			if (index < size) {
				BigArray<T>.Copy(items, index + 1, items, index, size - index);
			}
		}

		T IList<T>.this[int index] {
			get { return this[index]; }
			set { this[index] = value; }
		}

		public T this[long index] {
			get { return items[index]; }
			set { items[index] = value; }
		}

		#region Enumerator

		class Enumerator : IEnumerator<T> {
			private readonly BigList<T> list;
			private long offset;
			private long count;

			public Enumerator(BigList<T> list) {
				this.list = list;
			}

			public bool MoveNext() {
				return ++offset < count;
			}

			public void Reset() {
				count = list.size;
				offset = -1;
			}

			public T Current => list.items[offset];

			object IEnumerator.Current {
				get { return Current; }
			}

			public void Dispose() {
			}
		}

		#endregion
	}
}