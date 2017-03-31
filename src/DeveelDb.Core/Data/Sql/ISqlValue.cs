﻿// 
//  Copyright 2010-2016 Deveel
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

namespace Deveel.Data.Sql {
	/// <summary>
	/// The value that represents a SQL object value
	/// </summary>
	public interface ISqlValue : IComparable, IComparable<ISqlValue> {
		/// <summary>
		/// Gets a boolean value indicating if the value is <c>NULL</c>.
		/// </summary>
		/// <seealso cref="SqlNull"/>
		bool IsNull { get; }

		/// <summary>
		/// Checks if the current object is comparable with the given one.
		/// </summary>
		/// <param name="other">The other <see cref="ISqlValue"/> to compare.</param>
		/// <returns>
		/// Returns <c>true</c> if the current object is comparable
		/// with the given one, <c>false</c> otherwise.
		/// </returns>
		bool IsComparableTo(ISqlValue other);
	}
}