using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Deveel.Data.Sql.Tables {
	public class TableInfo : IDbObjectInfo {
		public TableInfo(ObjectName tableName) {
			if (tableName == null)
				throw new ArgumentNullException(nameof(tableName));

			TableName = tableName;
			Columns = new ColumnList();
		}

		DbObjectType IDbObjectInfo.ObjectType => DbObjectType.Table;

		ObjectName IDbObjectInfo.FullName => TableName;

		public ObjectName TableName { get; }

		public int TableId { get; set; }

		// TODO: this must be changed
		public virtual IColumnList Columns { get; }

		#region ColumnList

		class ColumnList : Collection<ColumnInfo>, IColumnList {
			public int IndexOf(ObjectName columnName) {
				throw new NotImplementedException();
			}

			public int IndexOf(string columnName) {
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}