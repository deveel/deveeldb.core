using System;

namespace Deveel.Data.Sql.Constraints {
	public sealed class ForeignKeyConstraintInfo : ConstraintInfo {
		public string[] ColumnNames { get; }

		public ObjectName ReferencedTableName { get; }

		public string[] ReferencedColumnNames { get; }

		public ForeignKeyAction Action { get; }

		public ForeignKeyConstraintInfo(string constraintName, ObjectName tableName, string[] columnNames, ObjectName refTableName, string[] refColumnNames, ForeignKeyAction action)
			: base(constraintName, ConstraintType.ForeignKey, tableName) {
			ColumnNames = columnNames;
			ReferencedTableName = refTableName;
			ReferencedColumnNames = refColumnNames;
			Action = action;
		}
	}
}