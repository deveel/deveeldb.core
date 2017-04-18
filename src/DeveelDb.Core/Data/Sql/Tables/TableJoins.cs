using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Tables {
	static class TableJoins {
		public static Task<ITable> JoinAsync(IContext context, ITable table, ITable other, ObjectName columnName, SqlExpressionType operatorType,
			SqlExpression expression) {
			var rightExpression = expression;
			// If the rightExpression is a simple variable then we have the option
			// of optimizing this join by putting the smallest table on the LHS.
			var rhsVar = (rightExpression as SqlReferenceExpression)?.ReferenceName;
			var lhsVar = columnName;
			var op = operatorType;

			if (rhsVar != null) {
				// We should arrange the expression so the right table is the smallest
				// of the sides.
				// If the left result is less than the right result

				if (table.RowCount < other.RowCount) {
					// Reverse the join
					rightExpression = SqlExpression.Reference(lhsVar);
					lhsVar = rhsVar;
					op = op.Reverse();

					// Reverse the tables.
					var t = other;
					other = table;
					table = t;
				}
			}

			var joinExp = SqlExpression.Binary(op, SqlExpression.Reference(lhsVar), rightExpression);

			// The join operation.
			return SimpleJoinAsync(context, table, other, joinExp);
		}

		public static async Task<ITable> SimpleJoinAsync(IContext context, ITable thisTable, ITable other, SqlBinaryExpression binary) {
			var objRef = binary.Left as SqlReferenceExpression;
			if (objRef == null)
				throw new ArgumentException();

			// Find the row with the name given in the condition.
			int lhsColumn = thisTable.TableInfo.Columns.IndexOf(objRef.ReferenceName);

			if (lhsColumn == -1)
				throw new Exception("Unable to find the LHS column specified in the condition: " + objRef.ReferenceName);

			// The join algorithm.  It steps through the RHS expression, selecting the
			// cells that match the relation from the LHS table (this table).

			var thisRowSet = new BigList<long>();
			var tableRowSet = new BigList<long>();

			var e = other.GetEnumerator();

			while (e.MoveNext()) {
				var rowIndex = e.Current.Id.Number;

				var rowResolver = new RowReferenceResolver(other, rowIndex);
				IEnumerable<long> selectedSet;

				using (var rowContext = context.Create($"row_{rowIndex}")) {
					rowContext.RegisterInstance<IReferenceResolver>(rowResolver);

					// Select all the rows in this table that match the joining condition.
					selectedSet = await thisTable.SelectRowsAsync(rowContext, binary);
				}

				var selectList = selectedSet.ToBigList();

				var size = selectList.Count;
				// Include in the set.
				for (int i = 0; i < size; i++) {
					tableRowSet.Add(rowIndex);
				}

				thisRowSet.AddRange(selectList);
			}

			// Create the new VirtualTable with the joined tables.

			var tabs = new[] { thisTable, other };
			var rowSets = new IEnumerable<long>[] { thisRowSet, tableRowSet };

			return new VirtualTable(tabs, rowSets);
		}
	}
}