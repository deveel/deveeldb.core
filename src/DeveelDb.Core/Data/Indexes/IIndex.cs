using System;
using System.Collections.Generic;

namespace Deveel.Data.Indexes {
	public interface IIndex<TKey, TValue> : IEnumerable<TValue> {
		/// <summary>
		/// Gets or sets a value indicating if the index is read-only.
		/// </summary>
		/// <remarks>
		/// After the index is set as read-only, any modification will
		/// cause an error to be thrown.
		/// </remarks>
		bool IsReadOnly { get; set; }

		/// <summary>
		/// Gets the number of elements in the index.
		/// </summary>
		long  Count { get; }

		/// <summary>
		/// Gets the element at the given index.
		/// </summary>
		/// <param name="index">The index of the element to get.</param>
		/// <returns>
		/// Returns the element stored at the given <paramref name="index"/>
		/// within the index.
		/// </returns>
		/// <exception cref="IndexOutOfRangeException">
		/// If the given <paramref name="index"/> is out of range.
		/// </exception>
		TValue this[long index] { get; }

		/// <summary>
		/// Adds a value to the end of the index.
		/// </summary>
		/// <param name="value">The value to add.</param>
		void Add(TValue value);

		///<summary>
		/// Inserts an element to the given position in the index.
		///</summary>
		///<param name="index"></param>
		///<param name="value"></param>
		/// <remarks>
		/// Any values after the given position are shifted forward.
		/// </remarks>
		/// <exception cref="IndexOutOfRangeException">
		/// If the position is out of bounds.
		/// </exception>
		void Insert(long index, TValue value);

		/// <summary>
		/// Removes the element at the given position from the index.
		/// </summary>
		/// <param name="index">The position within the index from where 
		/// to remove the value.</param>
		/// <returns>
		/// Returns the value that was removed from the given <paramref name="index"/>.
		/// </returns>
		/// <exception cref="IndexOutOfRangeException">
		/// If the given <paramref name="index"/> is out of range.
		/// </exception>
		TValue RemoveAt(long index);

		/// <summary>
		/// Checks if the given value is present within the index.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <remarks>
		/// This method assumes the list is sorted.
		/// <para>
		/// If the index is not sorted then this may return <b>false</b>
		/// even if the index does contain the value.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns <b>true</b> if the given <paramref name="value"/> is found,
		/// otherwise <b>false</b>.
		/// </returns>
		bool Contains(TValue value);

		/// <summary>
		/// Inserts a value in a sorted order in the index only if not already present.
		/// </summary>
		/// <param name="value">The value to insert to the index.</param>
		/// <returns>
		/// Returns <b>true</b> if the given <paramref name="value"/> has been
		/// successfully inserted, otherwise (if already present in the list)
		/// returns <b>false</b>.
		/// </returns>
		bool InsertSort(TValue value);

		/// <summary>
		/// Removes a value in a sorted order from the list only if already present.
		/// </summary>
		/// <param name="value">The value to remove from the index.</param>
		/// <returns>
		/// Returns <b>true</b> if the given <paramref name="value"/> has been
		/// successfully removed, otherwise (if not already present in the list)
		/// returns <b>false</b>.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// If the value removed differs from the given <paramref name="value"/>.
		/// </exception>
		bool RemoveSort(TValue value);

		/// <summary>
		/// Checks if the given key is present within the index.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="comparer"></param>
		/// <remarks>
		/// This method assumes the list is sorted.
		/// If the list is not sorted then this may return <b>false</b>
		/// even if the list does contain the key.
		/// </remarks>
		/// <returns>
		/// Returns <b>true</b> if the given <paramref name="key"/> is found,
		/// otherwise <b>false</b>.
		/// </returns>
		bool Contains(TKey key, IIndexComparer<TKey, TValue> comparer);

		/// <summary>
		/// Inserts the key/value pair into the index at the correct 
		/// sorted position determined by the given comparer.
		/// </summary>
		/// <param name="key">The key of the value to insert.</param>
		/// <param name="value">The value to insert.</param>
		/// <param name="comparer">The comparer used to determinate the correct sorted 
		/// order to add the given value.</param>
		/// <remarks>
		/// If the list already contains identical key then the value is add 
		/// to the end of the set of identical values in the list. 
		/// This way, the sort is stable (the order of identical elements does 
		/// not change).
		/// </remarks>
		void InsertSort(TKey key, TValue value, IIndexComparer<TKey, TValue> comparer);

		/// <summary>
		/// Removes the key/value pair from the list at the correct 
		/// sorted position determined by the given comparer.
		/// </summary>
		/// <param name="key">The key of the value to remove.</param>
		/// <param name="value">The value to remove.</param>
		/// <param name="comparer">The comparer used to determinate the correct sorted 
		/// order to remove the given value.</param>
		/// <returns>
		/// Returns the index within the list of the value removed.
		/// </returns>
		TValue RemoveSort(TKey key, TValue value, IIndexComparer<TKey, TValue> comparer);


		/// <summary>
		/// Searches the last value for the given key.
		/// </summary>
		/// <param name="key">The key of the value to return.</param>
		/// <param name="comparer">The comparer used to determinate the
		/// last value in the set to return.</param>
		/// <returns>
		/// Returns the index of the last value in the set for the given
		/// <paramref name="key"/>.
		/// </returns>
		long SearchLast(TKey key, IIndexComparer<TKey, TValue> comparer);

		/// <summary>
		/// Searches the first value for the given key.
		/// </summary>
		/// <param name="key">The key of the value to return.</param>
		/// <param name="comparer">The comparer used to determinate the
		/// first value in the set to return.</param>
		/// <returns>
		/// Returns the index of the first value in the set for the given
		/// <paramref name="key"/>.
		/// </returns>
		long SearchFirst(TKey key, IIndexComparer<TKey, TValue> comparer);

		/// <summary>
		/// Gets an object to enumerates all the element contained in
		/// the current index.
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="IIndexEnumerator{T}"/> that is
		/// used to enumerate all elements in the index.
		/// </returns>
		new IIndexEnumerator<TValue> GetEnumerator();

		/// <summary>
		/// Gets an object to enumerates the element contained in
		/// the current index within the given start and end offsets.
		/// </summary>
		/// <param name="startOffset">The offset within the index from where
		/// to start enumerating.</param>
		/// <param name="endOffset">The offset within the index where to end the
		/// enumeration.</param>
		/// <returns>
		/// Returns an instance of <see cref="IIndexEnumerator{T}"/> that is
		/// used to enumerate all elements in the index.
		/// </returns>
		IIndexEnumerator<TValue> GetEnumerator(long startOffset, long endOffset);
	}
}