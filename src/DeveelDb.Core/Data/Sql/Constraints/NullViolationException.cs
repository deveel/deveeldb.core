using System;

namespace Deveel.Data.Sql.Constraints {
	public sealed class NullViolationException : ConstraintViolationException {
		internal NullViolationException(string constraintName,
			ObjectName tableName,
			string columnName,
			ConstraintDeferrability deferrability)
			: base(ConstraintType.NotNull, constraintName, deferrability) {
			TableName = tableName;
			ColumnName = columnName;
		}

		public ObjectName TableName { get; }

		public string ColumnName { get; }
	}
}