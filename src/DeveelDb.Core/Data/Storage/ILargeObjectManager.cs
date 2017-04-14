using System;

namespace Deveel.Data.Storage {
	public interface ILargeObjectManager {
		/// <summary>
		/// Creates a new large object from the underlying
		/// database of the session.
		/// </summary>
		/// <param name="maxSize">The max size of the object.</param>
		/// <param name="compressed">A flag indicating if the content of the
		/// object will be compressed.</param>
		/// <remarks>
		/// <para>
		/// Large objects are immutable once finalized and the content size
		/// cannot exceed the specified <paramref name="maxSize"/>.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="ILargeObject"/> that is allocated
		/// in the large-object storage of the underlying database of this session.
		/// </returns>
		/// <seealso cref="GetLargeObject"/>
		/// <seealso cref="ILargeObject"/>
		ILargeObject CreateLargeObject(long maxSize, bool compressed);

		/// <summary>
		/// Gets a large object identified by the given unique identifier.
		/// </summary>
		/// <param name="objectId">The unique identifier of the object to obtain.</param>
		/// <returns>
		/// Returns an instance of <see cref="ILargeObject"/> identified by the given
		/// <paramref name="objectId"/> within the underlying database of this session.
		/// </returns>
		/// <seealso cref="ObjectId"/>
		ILargeObject GetLargeObject(ObjectId objectId);
	}
}