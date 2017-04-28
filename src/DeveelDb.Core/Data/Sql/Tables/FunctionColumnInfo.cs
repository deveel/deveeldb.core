using System;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Tables {
	public sealed class FunctionColumnInfo {
		public FunctionColumnInfo(SqlExpression function, string columnName, SqlType columnType) 
			: this(function, columnName, columnType, false) {
		}

		public FunctionColumnInfo(SqlExpression function, string columnName, SqlType columnType, bool reduced) {
			if (function == null)
				throw new ArgumentNullException(nameof(function));
			if (columnType == null)
				throw new ArgumentNullException(nameof(columnType));
			if (String.IsNullOrWhiteSpace(columnName))
				throw new ArgumentNullException(nameof(columnName));

			Function = function;
			IsReduced = reduced;

			ColumnInfo = new ColumnInfo(columnName, columnType);
		}

		public SqlExpression Function { get; }

		public bool IsReduced { get; }

		public ColumnInfo ColumnInfo { get; }

	}
}