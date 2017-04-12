using System;

namespace Deveel.Data.Sql.Constraints {
	public class ConstraintViolationException : SqlException {
		public ConstraintViolationException(ConstraintType constraintType, string constaintName, ConstraintDeferrability deferrability) {
			ConstraintType = constraintType;
			ConstraintName = constaintName;
			Deferrability = deferrability;
		}

		public ConstraintType ConstraintType { get; }

		public string ConstraintName { get; }

		public ConstraintDeferrability Deferrability { get; }
	}
}