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
using System.Collections.Generic;

namespace Deveel.Data.Indexes {
	/// <summary>
	/// An object that access to a set of indexes.
	/// </summary>
	/// <remarks>
	/// This will often expose an isolated snapshot of a set of indices 
	/// for a table.
	/// </remarks>
	public interface IIndexSet<TKey, TValue> : IEnumerable<IIndex<TKey, TValue>>, IDisposable {
		/// <summary>
		/// Gets a mutable implementation of <see cref="IIndex{TKey,TValue}"/>
		/// for the given index number in this set of indices.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		IIndex<TKey, TValue> GetIndex(int index);
	}
}