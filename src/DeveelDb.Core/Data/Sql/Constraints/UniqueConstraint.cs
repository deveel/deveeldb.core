using System;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Constraints {
	public sealed class UniqueConstraint : Constraint {
		public UniqueConstraint(UniqueConstraintInfo constraintInfo) 
			: base(constraintInfo) {
		}

		public new UniqueConstraintInfo ConstraintInfo => (UniqueConstraintInfo) base.ConstraintInfo;

		protected override void CheckConstraintViolation(IContext context, ITable table, long row, RowAction action) {
			if (!IsUnique(table, row, ConstraintInfo.ColumnNames, true))
				throw new UniqueViolationException(ConstraintName, TableName, ConstraintInfo.ColumnNames, Deferrability);
		}
	}
}