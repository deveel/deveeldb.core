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

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Tables {
	public sealed class ColumnInfo : ISqlFormattable {
		public ColumnInfo(string columnName, SqlType columnType) {
			if (String.IsNullOrWhiteSpace(columnName))
				throw new ArgumentNullException(nameof(columnName));
			if (columnType == null)
				throw new ArgumentNullException(nameof(columnType));

			ColumnName = columnName;
			ColumnType = columnType;
		}

		public string ColumnName { get; }

		public SqlType ColumnType { get; }

		public SqlExpression DefaultValue { get; set; }

		public bool HasDefaultValue => DefaultValue != null;

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			builder.Append(ColumnName);
			builder.Append(" ");
			ColumnType.AppendTo(builder);

			if (HasDefaultValue) {
				builder.Append(" DEFAULT ");
				DefaultValue.AppendTo(builder);
			}
		}
	}
}