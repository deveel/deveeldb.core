using System;
using System.Collections.Generic;

namespace Deveel.Data.Indexes {
	/// <summary>
	/// Enumerates the elements of an index.
	/// </summary>
	/// <remarks>
	/// Additionally to the functionalities inherited from <see cref="IEnumerator{T}"/>,
	/// it the removal from the underlying <see cref="IIndex{TKey,TValue}"/> of the element 
	/// at the current position of the enumeration (using <see cref="Remove"/>).
	/// </remarks>
	public interface IIndexEnumerator<T> : IEnumerator<T> {
		///<summary>
		/// Removes from the underlying index the current element this enumerator
		/// is positioned at.
		///</summary>
		/// <remarks>
		/// This method can be called only once per call to <see cref="IEnumerator{T}.Current"/>. 
		/// The behavior of an iterator is unspecified if the underlying index is modified while the 
		/// iteration is in progress in any way other than by calling this method.
		/// <para>
		/// Some implementations of <see cref="IIndexEnumerator{T}"/> may choose to not implement 
		/// this method, in which case an appropriate exception is generated.
		/// </para>
		/// </remarks>
		void Remove();
	}
}