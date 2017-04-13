using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Services;

namespace Deveel.Data.Sql.Constraints {
	public static class ContextExtensions {
		public static IEnumerable<Constraint> ResolveTableConstraints(this IContext context, ObjectName tableName) {
			var resolver = context.Scope.Resolve<ITableConstraintResolver>();
			if (resolver== null)
				return new Constraint[0];

			return resolver.ResolveConstraints(tableName);
		}

		public static bool IsNotNull(this IContext context, ObjectName tableName, string columnName) {
			var columns = context.ResolveTableConstraints(tableName);
			foreach (var constraint in columns) {
				if (constraint.ConstraintInfo.ConstraintType == ConstraintType.NotNull &&
				    ((NotNullConstraint) constraint).ConstraintInfo.ColumnNames.Any(x => x == columnName))
					return true;
			}

			return false;
		}
	}
}