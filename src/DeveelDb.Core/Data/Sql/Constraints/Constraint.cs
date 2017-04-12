using System;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Constraints {
	public abstract class Constraint : IDbObject, ISqlFormattable {
		protected Constraint(ConstraintInfo constraintInfo) {
			if (constraintInfo == null)
				throw new ArgumentNullException(nameof(constraintInfo));

			ConstraintInfo = constraintInfo;
		}

		public ConstraintInfo ConstraintInfo { get; }

		public string ConstraintName => ConstraintInfo.ConstraintName;

		public ConstraintDeferrability Deferrability => ConstraintInfo.Deferrability;

		public ObjectName TableName => ConstraintInfo.TableName;

		IDbObjectInfo IDbObject.ObjectInfo => ConstraintInfo;

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			ConstraintInfo.AppendTo(builder);
		}

		public override string ToString() {
			return this.ToSqlString();
		}

		public void CheckViolation(IContext context, ITable table, long row, RowAction action, ConstraintDeferrability deferrability) {
			if (deferrability != ConstraintInfo.Deferrability)
				return;

			if (!TableName.Equals(table.TableInfo.TableName))
				return;

			CheckConstraintViolation(context, table, row, action);
		}

		public void CheckViolation(IContext context, ITable table, IEnumerable<long> rows, RowAction action, ConstraintDeferrability deferrability) {
			if (deferrability != ConstraintInfo.Deferrability)
				return;

			if (!TableName.Equals(table.TableInfo.TableName))
				return;

			foreach (var row in rows) {
				CheckConstraintViolation(context, table, row, action);
			}
		}

		protected abstract void CheckConstraintViolation(IContext context, ITable table, long row, RowAction action);

		// TODO: need to implement indexes
		internal static bool IsUnique(ITable table, long row, string[] columnNames, bool allowNulls) {
			var tableInfo = table.TableInfo;

			// 'identical_rows' keeps a tally of the rows that match our added cell.
			BigList<long> identicalRows = null;

			// Resolve the list of column names to column indexes
			var colIndexes = columnNames.Select(x => tableInfo.Columns.IndexOf(x)).ToArray();

			foreach (var colIndex in colIndexes) {
				var value = table.GetValue(row, colIndex);

				// If the value being tested for uniqueness contains NULL, we return true
				// if nulls are allowed.
				if (value.IsNull)
					return allowNulls;

				// We are assured of uniqueness if 'identicalRows != null &&
				// identicalRows.Count == 0'  This is because 'identicalRows' keeps
				// a running tally of the rows in the table that contain unique columns
				// whose cells match the record being added.

				if (identicalRows == null || identicalRows.Count > 0) {
					//			// Ask SelectableScheme to return pointers to row(s) if there is
					//			// already a cell identical to this in the table.

					//			var index = table.GetIndex(colIndex);
					//			var list = index.SelectEqual(value).ToBigList();

					//			// If 'identicalRows' hasn't been set up yet then set it to 'list'
					//			// (the list of rows where there is a cell which is equal to the one
					//			//  being added)
					//			// If 'identicalRows' has been set up, then perform an
					//			// 'intersection' operation on the two lists (only keep the numbers
					//			// that are repeated in both lists).  Therefore we keep the rows
					//			// that match the row being added.

					//			if (identicalRows == null) {
					//				identicalRows = list;
					//			} else {
					//				list.Sort();
					//				var rowIndex = identicalRows.Count - 1;
					//				while (rowIndex >= 0) {
					//					var val = identicalRows[rowIndex];
					//					int foundIndex = list.BinarySearch(val);

					//					// If we _didn't_ find the index in the array
					//					if (foundIndex < 0 ||
					//					    list[foundIndex] != val) {
					//						identicalRows.RemoveAt(rowIndex);
					//					}
					//					--rowIndex;
					//				}
					//			}
				}
			}

			// If there is 1 (the row we added) then we are unique, otherwise we are
			// not.
			if (identicalRows != null) {
				var sz = identicalRows.Count;
				if (sz == 1)
					return true;
				if (sz > 1)
					return false;
				if (sz == 0)
					throw new InvalidOperationException("Assertion failed: We must be able to find the " +
														"row we are testing uniqueness against!");
			}

			return true;
		}
	}
}