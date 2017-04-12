using System;

namespace Deveel.Data.Sql.Constraints {
	public sealed class UniqueViolationException : ConstraintViolationException {
		internal UniqueViolationException(string constraintName, ObjectName tableName, string[] columnNames, ConstraintDeferrability deferrability)
			: base(ConstraintType.Unique, constraintName, deferrability) {
			TableName = tableName;
			ColumnNames = columnNames;
		}

		public ObjectName TableName { get; }

		public string[] ColumnNames { get; }
	}
}