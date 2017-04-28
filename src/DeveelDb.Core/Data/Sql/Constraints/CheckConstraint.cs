using System;

using Deveel.Data.Configuration;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Constraints {
	public class CheckConstraint : Constraint {
		public CheckConstraint(CheckConstraintInfo constraintInfo)
			: base(constraintInfo) {
		}

		public new CheckConstraintInfo ConstraintInfo => (CheckConstraintInfo) base.ConstraintInfo;

		protected override void CheckConstraintViolation(IContext context, ITable table, long row, RowAction action) {
			if (action == RowAction.Remove)
				return;

			var ignoreCase = context.GetValue("ignoreCase", true);
			var resolved = table.TableInfo.ResolveColumns(ConstraintInfo.Expression, ignoreCase);

			SqlExpression reduced;
			using (var rowContext = new Context(context, $"Check.Row(row)")) {
				var resolver = new RowReferenceResolver(table, row);
				(rowContext as IContext).Scope.Register<IReferenceResolver>(resolver);

				reduced = resolved.Reduce(rowContext);
			}

			if (reduced.ExpressionType != SqlExpressionType.Constant)
				throw new InvalidOperationException();

			var result = ((SqlConstantExpression) reduced).Value;

			if (!result.IsTrue)
				throw new CheckViolationException(ConstraintName, Deferrability);
		}
	}
}