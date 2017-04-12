using System;

namespace Deveel.Data.Sql.Constraints {
	public sealed class ForeignKeyViolationException : ConstraintViolationException {
		internal ForeignKeyViolationException(string constraintName,
			ObjectName tableName,
			string[] columnNames,
			ObjectName refTableName,
			string[] refColumnNames,
			ConstraintDeferrability deferrability)
			: base(ConstraintType.ForeignKey, constraintName, deferrability) {
			TableName = tableName;
			ColumnNames = columnNames;
			ReferencedTableName = refTableName;
			ReferencedColumnNames = refColumnNames;
		}

		public ObjectName TableName { get; }

		public string[] ColumnNames { get; }

		public ObjectName ReferencedTableName { get; }

		public string[] ReferencedColumnNames { get; }
	}
}