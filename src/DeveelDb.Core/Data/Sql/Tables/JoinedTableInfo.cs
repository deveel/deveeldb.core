using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public class JoinedTableInfo : TableInfo {
		private TableInfo[] tableInfos;
		
		public JoinedTableInfo(ObjectName tableName, TableInfo[] tableInfos) 
			: base(tableName) {
			this.tableInfos = tableInfos;
			Columns = new JoinedColumnList(this);
		}

		public override IColumnList Columns { get; }

		#region JoinedColumnList

		class JoinedColumnList : IColumnList {
			private readonly JoinedTableInfo tableInfo;
			private int[] columnTable;
			private int[] columnFilter;

			private List<ColumnInfo> columns;

			public JoinedColumnList(JoinedTableInfo tableInfo) {
				this.tableInfo = tableInfo;

				foreach (var info in tableInfo.tableInfos) {
					Count += info.Columns.Count;
				}

				columnTable = new int[Count];
				columnFilter = new int[Count];

				int index = 0;
				for (int i = 0; i < tableInfo.tableInfos.Length; ++i) {
					var curTableInfo = tableInfo.tableInfos[i];
					int refColCount = curTableInfo.Columns.Count;

					for (int n = 0; n < refColCount; ++n) {
						columnFilter[index] = n;
						columnTable[index] = i;
						++index;

						var columnInfo = curTableInfo.Columns[n];
						var newColumnInfo = new ColumnInfo(columnInfo.ColumnName, columnInfo.ColumnType) {
							DefaultValue = columnInfo.DefaultValue
						};

						columns.Add(newColumnInfo);
					}
				}
			}

			public IEnumerator<ColumnInfo> GetEnumerator() {
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			public void Add(ColumnInfo item) {
				throw new NotSupportedException();
			}

			public void Clear() {
				throw new NotSupportedException();
			}

			public bool Contains(ColumnInfo item) {
				throw new NotImplementedException();
			}

			public void CopyTo(ColumnInfo[] array, int arrayIndex) {
				throw new NotImplementedException();
			}

			public bool Remove(ColumnInfo item) {
				throw new NotSupportedException();
			}

			public int Count { get; }

			public bool IsReadOnly => true;

			public int IndexOf(ColumnInfo item) {
				return IndexOf(item.ColumnName);
			}

			public void Insert(int index, ColumnInfo item) {
				throw new NotSupportedException();
			}

			public void RemoveAt(int index) {
				throw new NotSupportedException();
			}

			public ColumnInfo this[int index] {
				get { return columns[index]; }
				set { throw new NotSupportedException(); }
			}

			public int IndexOf(ObjectName columnName) {
				int colIndex = 0;
				for (int i = 0; i < tableInfo.tableInfos.Length; ++i) {
					int col = tableInfo.tableInfos[i].Columns.IndexOf(columnName);
					if (col != -1)
						return col + colIndex;

					colIndex += tableInfo.tableInfos[i].Columns.Count;
				}
				return -1;
			}

			public int IndexOf(string columnName) {
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}