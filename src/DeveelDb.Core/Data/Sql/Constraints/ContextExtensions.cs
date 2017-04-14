using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Services;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Constraints {
	public static class ContextExtensions {
		public static IEnumerable<Constraint> QueryTableConstraints(this IContext context, ObjectName tableName) {
			var resolver = context.Scope.Resolve<ITableConstraintResolver>();
			if (resolver == null)
				return new Constraint[0];

			return resolver.ResolveConstraints(tableName);
		}


		public static void CheckNotNullViolation(this IContext context, ITable table, IEnumerable<long> rows) {
			var constraints = context.QueryTableConstraints(table.TableInfo.TableName).OfType<NotNullConstraint>();
			var rowsList = rows.ToBigArray();

			foreach (var constraint in constraints) {
				constraint.CheckViolation(context, table, rowsList, RowAction.Add, ConstraintDeferrability.InitiallyImmediate);
			}
		}

		public static void CheckAddConstraintViolations(this IContext context, ITable table, IEnumerable<long> rows,
			ConstraintDeferrability deferred) {
			context.CheckConstraintViolations(table, rows, RowAction.Add, deferred);
		}

		public static void CheckRemoveConstraintViolations(this IContext context, ITable table, IEnumerable<long> rows,
			ConstraintDeferrability deferred) {
			context.CheckConstraintViolations(table, rows, RowAction.Remove, deferred);
		}

		public static void CheckConstraintViolations(this IContext context, ITable table, IEnumerable<long> rows, RowAction action, ConstraintDeferrability deferred) {
			var tableName = table.TableInfo.TableName;
			var rowList = rows.ToBigArray();

			if (rowList.Length == 0)
				return;

			var constraints = context.QueryTableConstraints(tableName);
			foreach (var constraint in constraints) {
				if (deferred == ConstraintDeferrability.InitiallyDeferred ||
				    constraint.Deferrability == ConstraintDeferrability.InitiallyImmediate)

					constraint.CheckViolation(context, table, rowList, action, deferred);
			}
		}
	}
}