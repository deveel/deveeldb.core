using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Tables {
	public sealed class ColumnInfo : ISqlFormattable {
		public ColumnInfo(string columnName, SqlType columnType) {
			if (String.IsNullOrWhiteSpace(columnName))
				throw new ArgumentNullException(nameof(columnName));
			if (columnType == null)
				throw new ArgumentNullException(nameof(columnType));

			ColumnName = columnName;
			ColumnType = columnType;
		}

		public string ColumnName { get; }

		public SqlType ColumnType { get; }

		public SqlExpression DefaultValue { get; set; }

		public int Offset { get; internal set; }

		public bool HasDefaultValue => DefaultValue != null;

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			builder.Append(ColumnName);
			builder.Append(" ");
			ColumnType.AppendTo(builder);

			if (HasDefaultValue) {
				builder.Append(" DEFAULT ");
				DefaultValue.AppendTo(builder);
			}
		}
	}
}