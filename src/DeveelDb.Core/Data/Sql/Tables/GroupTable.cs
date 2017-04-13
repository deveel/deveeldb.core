using System;
using System.Collections.Generic;

using Deveel.Data.Services;

namespace Deveel.Data.Sql.Tables {
	public sealed class GroupTable : FunctionTable {
		private BigList<long> groupLinks;
		private BigList<long> groupLookup;
		private bool wholeTableAsGroup;
		private BigList<long> wholeTableGroup;
		private long wholeTableGroupSize;
		private bool wholeTableIsSimpleEnum;

		private GroupResolver groupResolver;

		public GroupTable(IContext context, FunctionTable table, ObjectName[] columns)
			: base(context, table) {

			if (columns != null && columns.Length > 0) {
				CreateMatrix(columns);
			} else {
				CreateGroup();
			}

			// Set up a group resolver for this method.
			groupResolver = new GroupResolver(this);
		}

		private void CreateGroup() {
			// TODO: create a new table ...
			wholeTableAsGroup = true;

			wholeTableGroupSize = Table.RowCount;

			// Set up 'whole_table_group' to the list of all rows in the reference
			// table.
			var en = Table.GetEnumerator();
			wholeTableIsSimpleEnum = en is SimpleRowEnumerator;
			if (!wholeTableIsSimpleEnum) {
				wholeTableGroup = new BigList<long>(Table.RowCount);
				while (en.MoveNext()) {
					wholeTableGroup.Add(en.Current.Id.Number);
				}
			}
		}

		private void CreateMatrix(ObjectName[] columns) {
			var rootTable = Table;
			long rowCount = rootTable.RowCount;
			int[] colLookup = new int[columns.Length];
			for (int i = columns.Length - 1; i >= 0; --i) {
				colLookup[i] = rootTable.TableInfo.Columns.IndexOf(columns[i]);
			}

			var rowList = rootTable.OrderRowsByColumns(colLookup).ToBigArray();

			// 'row_list' now contains rows in this table sorted by the columns to
			// group by.

			// This algorithm will generate two lists.  The group_lookup list maps
			// from rows in this table to the group number the row belongs in.  The
			// group number can be used as an index to the 'group_links' list that
			// contains consequtive links to each row in the group until -1 is reached
			// indicating the end of the group;

			groupLookup = new BigList<long>(rowCount);
			groupLinks = new BigList<long>(rowCount);
			long currentGroup = 0;
			long previousRow = -1;
			for (long i = 0; i < rowCount; i++) {
				var rowIndex = rowList[i];

				if (previousRow != -1) {
					bool equal = true;
					// Compare cell in column in this row with previous row.
					for (int n = 0; n < colLookup.Length && equal; ++n) {
						var c1 = rootTable.GetValue(rowIndex, colLookup[n]);
						var c2 = rootTable.GetValue(previousRow, colLookup[n]);
						equal = (c1.CompareTo(c2) == 0);
					}

					if (!equal) {
						// If end of group, set bit 15
						groupLinks.Add(previousRow | 0x040000000);
						currentGroup = groupLinks.Count;
					} else {
						groupLinks.Add(previousRow);
					}
				}

				// groupLookup.Insert(row_index, current_group);
				PlaceAt(groupLookup, rowIndex, currentGroup);

				previousRow = rowIndex;
			}

			// Add the final row.
			groupLinks.Add(previousRow | 0x040000000);
		}

		private static void PlaceAt(BigList<long> list, long index, long value) {
			while (index > list.Count) {
				list.Add(0);
			}

			list.Insert(index, value);
		}

		protected override void PrepareRowContext(IContext context, long row) {
			var rowResolver = groupResolver.GetRowResolver(row);
			context.Scope.RegisterInstance<IGroupResolver>(rowResolver);
			base.PrepareRowContext(context, row);
		}

		public override VirtualTable GroupMax(ObjectName maxColumn) {
			var table = Table;

			IList<long> rowList;

			if (wholeTableAsGroup) {
				// Whole table is group, so take top entry of table.

				rowList = new BigList<long>(1);
				var rowEnum = table.GetEnumerator();
				if (rowEnum.MoveNext()) {
					rowList.Add(rowEnum.Current.Id.Number);
				} else {
					// MAJOR HACK: If the referencing table has no elements then we choose
					//   an arbitrary index from the reference table to merge so we have
					//   at least one element in the table.
					//   This is to fix the 'SELECT COUNT(*) FROM empty_table' bug.
					rowList.Add(Int32.MaxValue - 1);
				}
			} else if (table.RowCount == 0) {
				rowList = new BigList<long>(0);
			} else if (groupLinks != null) {
				// If we are grouping, reduce down to only include one row from each
				// group.
				if (maxColumn == null) {
					rowList = GetTopRowsFromEachGroup();
				} else {
					var colNum = Table.TableInfo.Columns.IndexOf(maxColumn);
					rowList = GetMaxFromEachGroup(colNum);
				}
			} else {
				// OPTIMIZATION: This should be optimized.  It should be fairly trivial
				//   to generate a Table implementation that efficiently merges this
				//   function table with the reference table.

				// This means there is no grouping, so merge with entire table,
				var rowCount = table.RowCount;
				rowList = new BigList<long>(rowCount);
				var en = table.GetEnumerator();
				while (en.MoveNext()) {
					rowList.Add(en.Current.Id.Number);
				}
			}

			// Create a virtual table that's the new group table merged with the
			// functions in this...

			var tabs = new[] { table, this };
			var rowSets = new IEnumerable<long>[] { rowList, rowList };

			return new VirtualTable(tabs, rowSets);
		}

		private IList<long> GetTopRowsFromEachGroup() {
			var extractRows = new BigList<long>();
			var size = groupLinks.Count;
			var take = true;

			for (int i = 0; i < size; ++i) {
				var r = groupLinks[i];
				if (take)
					extractRows.Add(r & 0x03FFFFFFF);

				take = (r & 0x040000000) != 0;
			}

			return extractRows;
		}

		private IList<long> GetMaxFromEachGroup(int colNum) {
			var refTab = Table;

			var extractRows = new BigList<long>();
			var size = groupLinks.Count;

			long toTakeInGroup = -1;
			SqlObject max = null;

			for (int i = 0; i < size; ++i) {
				var row = groupLinks[i];

				var actRIndex = row & 0x03FFFFFFF;
				var cell = refTab.GetValue(actRIndex, colNum);

				if (max == null || cell.CompareTo(max) > 0) {
					max = cell;
					toTakeInGroup = actRIndex;
				}

				if ((row & 0x040000000) != 0) {
					extractRows.Add(toTakeInGroup);
					max = null;
				}
			}

			return extractRows;
		}

		private long GetGroupSize(long groupNumber) {
			int groupSize = 1;
			var i = groupLinks[groupNumber];
			while ((i & 0x040000000) == 0) {
				++groupSize;
				++groupNumber;
				i = groupLinks[groupNumber];
			}
			return groupSize;
		}

		private BigList<long> GetGroupRows(long groupNumber) {
			var rows = new BigList<long>();
			var row = groupLinks[groupNumber];

			while ((row & 0x040000000) == 0) {
				rows.Add(row);
				++groupNumber;
				row = groupLinks[groupNumber];
			}

			rows.Add(row & 0x03FFFFFFF);
			return rows;
		}

		private long GetRowGroup(long rowIndex) {
			return groupLookup[rowIndex];
		}

		#region GroupResolver

		class GroupResolver : IGroupResolver {
			private BigList<long> group;
			private IReferenceResolver groupRefResolver;
			private readonly GroupTable table;

			private GroupResolver(GroupTable table, long groupId) {
				this.table = table;
				GroupId = groupId;
			}

			public GroupResolver(GroupTable table)
				: this(table, -1) {
			}

			public long Size {
				get {
					if (GroupId == -2)
						return table.wholeTableGroupSize;
					if (group != null)
						return group.Count;

					return table.GetGroupSize(GroupId);
				}
			}

			public long GroupId { get; }

			private void EnsureGroup() {
				if (group == null) {
					if (GroupId == -2) {
						group = table.wholeTableGroup;
					} else {
						group = table.GetGroupRows(GroupId);
					}
				}
			}

			public IGroupResolver GetRowResolver(long row) {
				if (table.wholeTableAsGroup) {
					return new GroupResolver(table, -2);
				}

				var groupId = table.GetRowGroup(row);
				return new GroupResolver(table, groupId);
			}

			public SqlObject ResolveReference(ObjectName reference, long index) {
				int colIndex = table.Table.TableInfo.Columns.IndexOf(reference);
				if (colIndex == -1)
					throw new InvalidOperationException($"Column {reference} not found in table {table.Table.TableInfo.TableName}.");

				EnsureGroup();

				var rowIndex = index;
				if (group != null)
					rowIndex = group[index];

				return table.Table.GetValue(rowIndex, colIndex);
			}

			public IReferenceResolver GetResolver(long index) {
				return new ReferenceResolver(this, index);
			}

			#region ReferenceResolver

			class ReferenceResolver : IReferenceResolver {
				private readonly GroupResolver groupResolver;
				private readonly long index;

				public ReferenceResolver(GroupResolver groupResolver, long index) {
					this.groupResolver = groupResolver;
					this.index = index;
				}

				public SqlObject ResolveReference(ObjectName referenceName) {
					return groupResolver.ResolveReference(referenceName, index);
				}

				public SqlType ReturnType(ObjectName referenceName) {
					var columnOffset = groupResolver.table.TableInfo.Columns.IndexOf(referenceName);
					if (columnOffset < 0)
						throw new InvalidOperationException($"Cannot find column {referenceName} in table {groupResolver.table.TableInfo.TableName}");

					return groupResolver.table.TableInfo.Columns[columnOffset].ColumnType;
				}
			}

			#endregion
		}

		#endregion
	}
}