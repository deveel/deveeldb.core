using System;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Constraints {
	public sealed class ForeignKeyConstraint : Constraint {
		public ForeignKeyConstraint(ForeignKeyConstraintInfo constraintInfo)
			: base(constraintInfo) {
		}

		public new ForeignKeyConstraintInfo ConstraintInfo => (ForeignKeyConstraintInfo) base.ConstraintInfo;

		private void CheckRemoveViolation(IContext context, ITable table, long row) {
			// TODO: Count the references and throw if any
			throw new NotImplementedException();
		}

		private void CheckAddViolation(IContext context, ITable table, long row) {
			// TODO: Count the references and throw if none

			throw new NotImplementedException();
		}

		protected override void CheckConstraintViolation(IContext context, ITable table, long row, RowAction action) {
			if (action == RowAction.Add) {
				CheckAddViolation(context, table, row);
			} else {
				CheckRemoveViolation(context, table, row);
			}
		}
	}
}