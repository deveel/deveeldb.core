using System;

namespace Deveel.Data.Indexes {
	/// <summary>
	/// An object that access to a set of indexes.
	/// </summary>
	/// <remarks>
	/// This will often expose an isolated snapshot of a set of indices 
	/// for a table.
	/// </remarks>
	public interface IIndexSet<TKey, TValue> : IDisposable {
		/// <summary>
		/// Gets a mutable implementation of <see cref="IIndex{TKey,TValue}"/>
		/// for the given index number in this set of indices.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		IIndex<TKey, TValue> GetIndex(int index);
	}
}