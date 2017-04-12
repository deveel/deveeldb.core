using System;

namespace Deveel.Data.Sql.Constraints {
	public sealed class NotNullConstraintInfo : ConstraintInfo {
		public NotNullConstraintInfo(string constraintName, ObjectName tableName, string[] columnNames) 
			: base(constraintName, ConstraintType.NotNull, tableName) {
			if (columnNames == null)
				throw new ArgumentNullException(nameof(columnNames));
			if (columnNames.Length == 0)
				throw new ArgumentException();

			ColumnNames = columnNames;
		}

		public string[] ColumnNames { get; }

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("NOT NULL ");
			builder.Append(ConstraintName);
			builder.Append(" ON ");
			TableName.AppendTo(builder);
			builder.Append("(");

			for (int i = 0; i < ColumnNames.Length; i++) {
				builder.Append(ColumnNames[i]);

				if (i < ColumnNames.Length - 1)
					builder.Append(", ");
			}

			builder.Append(")");
		}
	}
}