using System;

namespace Deveel.Data.Sql.Constraints {
	public sealed class CheckViolationException : ConstraintViolationException {
		internal CheckViolationException(string constraintName, ConstraintDeferrability deferrability)
			: base(ConstraintType.Check, constraintName, deferrability) {
		}
	}
}