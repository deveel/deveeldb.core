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

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql {
	public sealed class SqlTableType : SqlType {
		internal SqlTableType(TableInfo tableInfo)
			: base(SqlTypeCode.Table) {
			if (tableInfo == null)
				throw new ArgumentNullException(nameof(tableInfo));

			TableInfo = tableInfo;
		}

		public TableInfo TableInfo { get; }

		public override bool IsInstanceOf(ISqlValue value) {
			return value is ITable;
		}

		public override bool IsIndexable => false;

		public override bool CanCastTo(ISqlValue value, SqlType destType) {
			if (!(value is ITable))
				return false;
			if (TableInfo.Columns.Count != 1)
				return false;

			var table = (ITable) value;
			var fieldValue = table.GetValue(0, 0);

			return fieldValue.CanCastTo(destType);
		}

		public override ISqlValue Cast(ISqlValue value, SqlType destType) {
			if (!(value is ITable))
				throw new NotSupportedException();

			var table = (ITable) value;

			if (table.TableInfo.Columns.Count != 1)
				throw new NotSupportedException();

			var fieldValue = table.GetValue(0, 0);
			if (!fieldValue.CanCastTo(destType))
				throw new NotSupportedException();

			return fieldValue.Type.Cast(fieldValue.Value, destType);
		}
	}
}