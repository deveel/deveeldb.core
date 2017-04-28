using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Constraints {
	public interface ITableConstraintResolver {
		IEnumerable<Constraint> ResolveConstraints(ObjectName tableName);			
	}
}