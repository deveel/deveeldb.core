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
	/// A single parameter value in a <see cref="SqlQuery"/>.
	/// </summary>
	public sealed class QueryParameter {
		public QueryParameter(SqlType sqlType) 
			: this(sqlType, null) {
		}

		public QueryParameter(SqlType sqlType, ISqlValue value) 
			: this(Marker, sqlType, value) {
		}

		public QueryParameter(string name, SqlType sqlType) 
			: this(name, sqlType, null) {
		}

		public QueryParameter(string name, SqlType sqlType, ISqlValue value) {
			if (sqlType == null)
				throw new ArgumentNullException("sqlType");

			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (!String.Equals(name, Marker, StringComparison.Ordinal) &&
			    name[0] == NamePrefix) {
				name = name.Substring(1);

				if (String.IsNullOrEmpty(name))
					throw new ArgumentException("Cannot specify only the variable bind prefix as parameter.");
			}

			Name = name;
			SqlType = sqlType;
			Value = value;
			Direction = QueryParameterDirection.In;
		}

		public const char NamePrefix = ':';
		public const string Marker = "?";

		public string Name { get; private set; }

		public SqlType SqlType { get; private set; }

		public QueryParameterDirection Direction { get; set; }

		public ISqlValue Value { get; set; }
	}
}