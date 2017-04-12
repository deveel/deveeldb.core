using System;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Constraints {
	public class CheckConstraint : Constraint {
		public CheckConstraint(CheckConstraintInfo constraintInfo)
			: base(constraintInfo) {
		}

		public new CheckConstraintInfo ConstraintInfo => (CheckConstraintInfo) base.ConstraintInfo;

		protected override void CheckConstraintViolation(IContext context, ITable table, long row, RowAction action) {
			// TODO: qualify column references in the expression
			// TODO: reduce the expression to a constant boolean
			throw new NotImplementedException();
		}
	}
}