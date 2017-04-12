using System;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Constraints {
	public sealed class NotNullConstraint : Constraint {
		public NotNullConstraint(NotNullConstraintInfo constraintInfo) 
			: base(constraintInfo) {
		}

		public new NotNullConstraintInfo ConstraintInfo => (NotNullConstraintInfo) base.ConstraintInfo;

		protected override void CheckConstraintViolation(IContext context, ITable table, long row, RowAction action) {
			if (action == RowAction.Remove)
				return;

			foreach (var columnName in ConstraintInfo.ColumnNames) {
				var columnIndex = table.TableInfo.Columns.IndexOf(columnName);
				var value = table.GetValue(row, columnIndex);

				if (value.IsNull)
					throw new NullViolationException(ConstraintName, TableName, columnName, Deferrability);
			}
			
		}
	}
}