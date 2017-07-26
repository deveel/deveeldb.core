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

namespace Deveel.Data.Storage {
	public interface ILargeObjectManager {
		/// <summary>
		/// Creates a new large object from the underlying
		/// database of the session.
		/// </summary>
		/// <param name="storeId">The identificator of the store that handles
		/// the large object created</param>
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
		ILargeObject CreateLargeObject(int storeId, long maxSize, bool compressed);

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