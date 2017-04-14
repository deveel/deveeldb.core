using System;

using Antlr4.Runtime.Atn;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Constraints {
	public sealed class ForeignKeyConstraint : Constraint {
		public ForeignKeyConstraint(ForeignKeyConstraintInfo constraintInfo)
			: base(constraintInfo) {
		}

		public new ForeignKeyConstraintInfo ConstraintInfo => (ForeignKeyConstraintInfo) base.ConstraintInfo;

		public string[] ColumnNames => ConstraintInfo.ColumnNames;

		public ObjectName ForeignTableName => ConstraintInfo.ForeignTableName;

		public string[] ForeignColumnNames => ConstraintInfo.ForeignColumnNames;

		private void CheckRemoveViolation(IContext context, ITable table, long row) {
			// Return the count of records where the given row of
			//   table_name(columns, ...) IN
			//                    ref_table_name(ref_columns, ...)
			int rowCount = RowCountOfReferenceTable(context, row, TableName, ColumnNames, ForeignTableName, ForeignColumnNames,
				false);
			if (rowCount == -1) {
				// foreign key is NULL
			}

			if (rowCount == 0) {
				throw new ForeignKeyViolationException(ConstraintName, TableName, ColumnNames, ForeignTableName, ForeignColumnNames,
					Deferrability);
			}
		}

		private void CheckAddViolation(IContext context, ITable table, long row) {
			// Make sure the referenced record exists

			// Return the count of records where the given row of
			//   ref_table_name(columns, ...) IN
			//                    table_name(ref_columns, ...)
			int rowCount = RowCountOfReferenceTable(context,
				row,
				ForeignTableName, ForeignColumnNames,
				TableName, ColumnNames,
				true);

			// There must be 0 references otherwise the delete isn't allowed to
			// happen.
			if (rowCount > 0) {
				throw new ForeignKeyViolationException(ConstraintName, TableName, ColumnNames, ForeignTableName, ForeignColumnNames,
					Deferrability);
			}
		}

		private static int RowCountOfReferenceTable(IContext context, long rowIndex, ObjectName table1,
			string[] cols1, ObjectName table2, String[] cols2,
			bool checkSourceTableKey) {

			//// Get the tables
			//var t1 = context.GetTable(table1);
			//var t2 = context.GetTable(table2);

			//// The table defs
			//var dti1 = t1.TableInfo;
			//var dti2 = t2.TableInfo;

			//// Resolve the list of column names to column indexes
			//var col1Indexes = dti1.IndexOfColumns(cols1).ToArray();
			//var col2Indexes = dti2.IndexOfColumns(cols2).ToArray();

			//int keySize = col1Indexes.Length;

			//// Get the data from table1
			//var keyValue = new Field[keySize];
			//int nullCount = 0;
			//for (int n = 0; n < keySize; ++n) {
			//	keyValue[n] = t1.GetValue(rowIndex, col1Indexes[n]);
			//	if (keyValue[n].IsNull) {
			//		++nullCount;
			//	}
			//}

			//// If we are searching for null then return -1;
			//if (nullCount > 0)
			//	return -1;

			//// HACK: This is a hack.  The purpose is if the key exists in the source
			////   table we return 0 indicating to the delete check that there are no
			////   references and it's valid.  To the semantics of the method this is
			////   incorrect.
			//if (checkSourceTableKey) {
			//	var keys = t1.FindKeys(col1Indexes, keyValue);
			//	if (keys.Any())
			//		return 0;
			//}

			//return t2.FindKeys(col2Indexes, keyValue).Count();

			throw new NotImplementedException();
		}

		protected override void CheckConstraintViolation(IContext context, ITable table, long row, RowAction action) {
			if (action == RowAction.Add) {
				CheckAddViolation(context, table, row);
			} else {
				CheckRemoveViolation(context, table, row);
			}
		}
	}
}