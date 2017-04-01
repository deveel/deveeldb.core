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

namespace Deveel.Data.Security {
	/// <summary>
	/// Enumeration of the privileges that can be
	/// assigned to a user or a role over given
	/// database objects.
	/// </summary>
	[Flags]
	public enum Privileges {
		/// <summary>
		/// Empry privileges (no privileges)
		/// </summary>
		None = 0,

		/// <summary>
		/// The user or role can <c>CREATE</c> an object in a schema
		/// </summary>
		Create = 1,

		/// <summary>
		/// The user or role can <c>ALTER</c> a database object
		/// </summary>
		Alter = 2,

		/// <summary>
		/// The user or role can <c>DROP</c> a database object
		/// </summary>
		Drop = 4,

		/// <summary>
		/// The user or role can <c>LIST</c> the objects in a schema
		/// </summary>
		List = 8,

		/// <summary>
		/// The user or role can <c>SELECT</c> data from a table
		/// </summary>
		Select = 16,

		/// <summary>
		/// The user or role can <c>UPDATE</c> the data in a table
		/// </summary>
		Update = 32,

		/// <summary>
		/// The user or role can <c>DELETE</c> data from a table
		/// </summary>
		Delete = 64,

		/// <summary>
		/// The user or role can <c>INSERT</c> data in a table
		/// </summary>
		Insert = 128,

		/// <summary>
		/// The user or role can <c>REFERENCE</c> a table in a foreign
		/// key constraint
		/// </summary>
		References = 256,

		Usage = 512,

		/// <summary>
		/// The user or role can <c>COMPACT</c> a table
		/// </summary>
		Compact = 1024,

		/// <summary>
		/// The user or role can <c>EXECUTE</c> a program
		/// </summary>
		Execute = 2048,


		// Sets
		TableAll = Select |
		           Update |
		           Delete |
		           Insert |
		           References |
		           Usage |
		           Compact,

		TableRead = Select | Usage,

		SchemaAll = Create |
		            Alter |
		            Drop |
		            List,

		SchemaRead = List
	}
}