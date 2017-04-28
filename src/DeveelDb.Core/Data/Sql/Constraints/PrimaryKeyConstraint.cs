using System;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Constraints {
	public sealed class PrimaryKeyConstraint : Constraint {
		public PrimaryKeyConstraint(PrimaryKeyConstraintInfo constraintInfo)
			: base(constraintInfo) {
		}

		public new PrimaryKeyConstraintInfo ConstraintInfo => (PrimaryKeyConstraintInfo) base.ConstraintInfo;

		protected override void CheckConstraintViolation(IContext context, ITable table, long row, RowAction action) {
			if (action == RowAction.Remove)
				return;

			if (!IsUnique(table, row, new []{ConstraintInfo.ColumnName}, false))
				throw new PrimaryKeyViolationException(ConstraintName, TableName, ConstraintInfo.ColumnName, Deferrability);
		}
	}
}