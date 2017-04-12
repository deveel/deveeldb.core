using System;

namespace Deveel.Data.Sql.Indexes {
	public sealed class IndexInfo : IDbObjectInfo {
		public IndexInfo(ObjectName indexName, ObjectName tableName, string[] columnNames) {
			IndexName = indexName;
			TableName = tableName;
			ColumnNames = columnNames;
		}

		DbObjectType IDbObjectInfo.ObjectType => DbObjectType.Index;

		public ObjectName IndexName { get; }

		ObjectName IDbObjectInfo.FullName => IndexName;

		public ObjectName TableName { get; }

		public string[] ColumnNames { get; }
	}
}