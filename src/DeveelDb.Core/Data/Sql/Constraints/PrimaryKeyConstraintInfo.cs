using System;

namespace Deveel.Data.Sql.Constraints {
	public sealed class PrimaryKeyConstraintInfo : ConstraintInfo {
		public PrimaryKeyConstraintInfo(string constraintName, ObjectName tableName, string columnName)
			: base(constraintName, ConstraintType.PrimaryKey, tableName) {
			if (String.IsNullOrWhiteSpace(columnName))
				throw new ArgumentNullException(nameof(columnName));

			ColumnName = columnName;
		}

		public string ColumnName { get; }

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("PRIMARY KEY");
			builder.Append(" ");
			builder.Append(ConstraintName);
			builder.Append(" ON ");
			TableName.AppendTo(builder);
			builder.Append("(");
			builder.Append(ColumnName);
			builder.Append(")");
		}
	}
}