// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Tables {
	static class TableJoins {
		public static Task<ITable> JoinAsync(IContext context, ITable table, ITable other, ObjectName columnName, SqlExpressionType operatorType,
			SqlExpression expression) {
			// If the rightExpression is a simple variable then we have the option
			// of optimizing this join by putting the smallest table on the LHS.
			var rhsVar = (expression as SqlReferenceExpression)?.ReferenceName;
			var lhsVar = columnName;
			var op = operatorType;

			if (rhsVar != null) {
				// We should arrange the expression so the right table is the smallest
				// of the sides.
				// If the left result is less than the right result

				if (table.RowCount < other.RowCount) {
					// Reverse the join
					expression = SqlExpression.Reference(lhsVar);
					lhsVar = rhsVar;
					op = op.Reverse();

					// Reverse the tables.
					var t = other;
					other = table;
					table = t;
				}
			}

			var joinExp = SqlExpression.Binary(op, SqlExpression.Reference(lhsVar), expression);

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

			foreach (var row in other) {
				var rowIndex = row.Id.Number;

				var rowResolver = row.GetResolver();
				IEnumerable<long> selectedSet;

				using (var rowContext = context.Create($"row_{rowIndex}", scope => scope.ReplaceInstance<IReferenceResolver>(rowResolver))) {
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

		public static ITable Join(ITable table, ITable otherTable, bool quick) {
			ITable outTable;

			if (quick) {
				// This implementation doesn't materialize the join
				outTable = new CrossTable(table, otherTable);
			} else {
				var tabs = new[] { table, otherTable };
				var rowSets = new BigList<long>[2];

				// Optimized trivial case, if either table has zero rows then result of
				// join will contain zero rows also.
				if (table.RowCount == 0 || otherTable.RowCount == 0) {
					rowSets[0] = new BigList<long>(0);
					rowSets[1] = new BigList<long>(0);
				} else {
					// The natural join algorithm.
					var thisRowSet = new BigList<long>();
					var tableRowSet = new BigList<long>();

					// Get the set of all rows in the given table.
					var tableSelectedSet = otherTable.Select(x => x.Id.Number).ToList();

					int tableSelectedSetSize = tableSelectedSet.Count;

					// Join with the set of rows in this table.
					using (var e = table.GetEnumerator()) {
						while (e.MoveNext()) {
							var rowIndex = e.Current.Id.Number;
							for (long i = 0; i < tableSelectedSetSize; ++i) {
								thisRowSet.Add(rowIndex);
							}

							tableRowSet.AddRange(tableSelectedSet);
						}
					}

					// The row sets we are joining from each table.
					rowSets[0] = thisRowSet;
					rowSets[1] = tableRowSet;
				}

				// Create the new VirtualTable with the joined tables.
				outTable = new VirtualTable(tabs, rowSets);
			}

			return outTable;
		}
	}
}