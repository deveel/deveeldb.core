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

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql {
	public sealed class SqlArray : ISqlValue, ISqlFormattable, IList, IEnumerable<SqlExpression> {
		private readonly SqlExpression[] expressions;

		public SqlArray(SqlExpression[] expressions) {
			this.expressions = expressions;
		}

		public int Length => expressions?.Length ?? 0;

		public SqlExpression this[int index] {
			get {
				if (index < 0 || index >= Length)
					throw new ArgumentOutOfRangeException(nameof(index));

				return expressions[index];
			}
		}

		int IComparable.CompareTo(object obj) {
			throw new NotSupportedException();
		}

		int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
			throw new NotSupportedException();
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return false;
		}

		public override string ToString() {
			return this.ToSqlString();
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			builder.Append("(");

			for (int i = 0; i < expressions.Length; i++) {
				(expressions[i] as ISqlFormattable).AppendTo(builder);

				if (i < expressions.Length-1)
					builder.Append(", ");
			}

			builder.Append(")");
		}

		public void CopyTo(Array array, int index) {
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (array.GetType().GetElementType() != typeof(SqlExpression))
				throw new ArgumentException("Cannot copy to an array that is not of SQL Expressions");

			if (expressions == null)
				return;

			if (Length + index > array.Length)
				throw new ArgumentException("Not enough capacity in the destination array");

			Array.Copy(expressions, 0, array, index, Length);
		}

		int ICollection.Count {
			get { return Length; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		int IList.Add(object value) {
			throw new NotSupportedException();
		}

		void IList.Clear() {
			throw new NotSupportedException();
		}

		bool IList.Contains(object value) {
			throw new NotSupportedException();
		}

		int IList.IndexOf(object value) {
			throw new NotSupportedException();
		}

		void IList.Insert(int index, object value) {
			throw new NotSupportedException();
		}

		void IList.Remove(object value) {
			throw new NotSupportedException();
		}

		void IList.RemoveAt(int index) {
			throw new NotSupportedException();
		}

		bool IList.IsFixedSize {
			get { return true; }
		}

		bool IList.IsReadOnly {
			get { return true; }
		}

		object IList.this[int index] {
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}

		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}

		public IEnumerator<SqlExpression> GetEnumerator() {
			return new ArrayEnumerator(this);
		}

		#region ArrayEnumerator

		class ArrayEnumerator : IEnumerator<SqlExpression> {
			private readonly SqlArray array;
			private int offset;
			private int length;

			public ArrayEnumerator(SqlArray array) {
				this.array = array;
				Reset();
			}

			public bool MoveNext() {
				return ++offset < length;
			}

			public void Reset() {
				offset = -1;
				length = array.Length;
			}

			public SqlExpression Current => array[offset];

			object IEnumerator.Current {
				get { return Current; }
			}

			public void Dispose() {
			}
		}

		#endregion
	}
}