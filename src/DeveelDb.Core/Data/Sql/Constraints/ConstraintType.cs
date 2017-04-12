using System;

namespace Deveel.Data.Sql.Constraints {
	public enum ConstraintType {
		NotNull = 1,
		PrimaryKey = 2,
		ForeignKey = 3,
		Check = 4,
		Unique = 5
	}
}