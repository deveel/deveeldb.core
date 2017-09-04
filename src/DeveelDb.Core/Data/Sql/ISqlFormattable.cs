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

namespace Deveel.Data.Sql {
	/// <summary>
	/// Provides a contact for representing an element into
	/// a proper SQL string
	/// </summary>
	/// <remarks>
	/// <para>
	/// Implementations of this interface provide an efficient
	/// way to construct complex representations of SQL statements
	/// and queries, expressions and other objects, by appending
	/// to a builder.
	/// </para>
	/// </remarks>
	public interface ISqlFormattable {
		/// <summary>
		/// Appends the string representation of the object
		/// into the builder.
		/// </summary>
		/// <param name="builder">The SQL string builder where to append
		/// the string representation of this object.</param>
		void AppendTo(SqlStringBuilder builder);
	}
}
