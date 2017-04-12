using System;

namespace Deveel.Data.Sql.Constraints {
	public enum ForeignKeyAction {
		None = 0,
		Cascade = 1,
		SetNull = 2,
		SetDefault = 3,
	}
}