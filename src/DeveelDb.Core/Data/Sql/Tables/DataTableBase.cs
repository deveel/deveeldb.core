using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Deveel.Data.Sql.Indexes;

namespace Deveel.Data.Sql.Tables {
	public abstract class DataTableBase : TableBase, IRootTable {
		private Index[] indexes;

		bool IEquatable<ITable>.Equals(ITable table) {
			return this == table;
		}

		protected override IEnumerable<long> ResolveRows(int column, IEnumerable<long> rows, ITable ancestor) {
			if (ancestor != this)
				throw new Exception("Method routed to incorrect table ancestor.");

			return rows;
		}

		protected override RawTableInfo GetRawTableInfo(RawTableInfo rootInfo) {
			var rows = this.Select(row => row.Id.Number).ToBigArray();
			rootInfo.Add(this, rows);
			return rootInfo;
		}

		protected override Index GetColumnIndex(int column, int originalColumn, ITable ancestor) {
			var index = GetColumnIndex(column);
			if (ancestor == this)
				return index;

			return index.Subset(ancestor, originalColumn);
		}

		public override Index GetColumnIndex(int column) {
			if (indexes == null)
				throw new InvalidOperationException("The indexes for the table were not built");

			return indexes[column];
		}

		protected void SetupIndexes(string indexTypeName) {
			Type indexType;
			if (String.Equals(indexTypeName, "NONE", StringComparison.OrdinalIgnoreCase)) {
				indexType = typeof(BlindSearchIndex);
			} else if (String.Equals(indexTypeName, "BLIST", StringComparison.OrdinalIgnoreCase)) {
				indexType = typeof(InsertSearchIndex);
			} else {
				indexType = Type.GetType(indexTypeName, false, true);
			}

			if (indexType == null) {
				indexType = typeof(BlindSearchIndex);
			} else if (!typeof(Index).GetTypeInfo().IsAssignableFrom(indexType.GetTypeInfo())) {
				throw new InvalidOperationException(String.Format("The type '{0}' is not a valid table index.", indexType));
			}

			SetupIndexes(indexType);
		}


		protected virtual void SetupIndexes(Type indexType) {
			indexes = new Index[TableInfo.Columns.Count];
			for (int i = 0; i < TableInfo.Columns.Count; i++) {
				var columnName = TableInfo.Columns[i].ColumnName;
				var indexInfo = new IndexInfo(new ObjectName(TableInfo.TableName, $"col_{i}"), TableInfo.TableName, columnName);
				if (indexType == typeof(BlindSearchIndex)) {
					indexes[i] = new BlindSearchIndex(indexInfo);
					indexes[i].AttachTo(this);
				} else if (indexType == typeof(InsertSearchIndex)) {
					indexes[i] = new InsertSearchIndex(indexInfo);
					indexes[i].AttachTo(this);
				} else {
					var index = Activator.CreateInstance(indexType, indexInfo) as Index;
					if (index == null)
						throw new InvalidOperationException();

					index.AttachTo(this);
					indexes[i] = index;
				}
			}
		}

		protected void AddRowToIndex(int rowNumber) {
			int colCount = TableInfo.Columns.Count;
			var tableInfo = TableInfo;
			for (int i = 0; i < colCount; ++i) {
				if (tableInfo.Columns[i].ColumnType.IsIndexable) {
					var index = GetColumnIndex(i);
					index.Insert(rowNumber);
				}
			}
		}

		protected void RemoveRowFromIndex(long rowNumber) {
			int colCount = TableInfo.Columns.Count;
			var tableInfo = TableInfo;
			for (int i = 0; i < colCount; ++i) {
				if (tableInfo.Columns[i].ColumnType.IsIndexable) {
					var index = GetColumnIndex(i);
					index.Remove((int)rowNumber);
				}
			}
		}
	}
}