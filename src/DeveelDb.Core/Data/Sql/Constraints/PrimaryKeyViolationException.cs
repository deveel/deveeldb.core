using System;

namespace Deveel.Data.Sql.Constraints {
	public sealed class PrimaryKeyViolationException : ConstraintViolationException {
		internal PrimaryKeyViolationException(string constaintName, ObjectName tableName, string columnName, ConstraintDeferrability deferrability) 
			: base(ConstraintType.PrimaryKey, constaintName, deferrability) {
			TableName = tableName;
			ColumnName = columnName;
		}

		public ObjectName TableName { get; }

		public string ColumnName { get; }
	}
}