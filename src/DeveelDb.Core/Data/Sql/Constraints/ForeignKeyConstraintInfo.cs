using System;
using System.Linq;

namespace Deveel.Data.Sql.Constraints {
	public sealed class ForeignKeyConstraintInfo : ConstraintInfo {
		public ForeignKeyConstraintInfo(string constraintName, ObjectName tableName, string[] columnNames, ObjectName foreignTableName, string[] foreignColumnNames, ForeignKeyAction action)
			: base(constraintName, ConstraintType.ForeignKey, tableName) {
			if (columnNames == null)
				throw new ArgumentNullException(nameof(columnNames));
			if (columnNames.Length == 0)
				throw new ArgumentException();

			if (foreignTableName == null)
				throw new ArgumentNullException(nameof(foreignTableName));
			if (foreignColumnNames == null)
				throw new ArgumentNullException(nameof(foreignColumnNames));
			if (foreignColumnNames.Length == 0)
				throw new ArgumentException();

			if (columnNames.Length != foreignColumnNames.Length)
				throw new ArgumentException();

			ColumnNames = columnNames;
			ForeignTableName = foreignTableName;
			ForeignColumnNames = foreignColumnNames;
			Action = action;
		}

		public string[] ColumnNames { get; }

		public ObjectName ForeignTableName { get; }

		public string[] ForeignColumnNames { get; }

		public ForeignKeyAction Action { get; }
	}
}