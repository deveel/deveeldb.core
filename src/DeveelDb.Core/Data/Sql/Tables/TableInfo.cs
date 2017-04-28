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
using System.Collections;
using System.Collections.Generic;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Indexes;

namespace Deveel.Data.Sql.Tables {
	public class TableInfo : IDbObjectInfo, ISqlFormattable {
		private bool readOnly;

		public TableInfo(ObjectName tableName) 
			: this(TableTypes.Table, tableName) {
		}

		public TableInfo(string type, ObjectName tableName) {
			if (tableName == null)
				throw new ArgumentNullException(nameof(tableName));
			if (String.IsNullOrEmpty(type))
				throw new ArgumentNullException(nameof(type));

			Type = type;
			TableName = tableName;
			TableId = -1;
			Columns = new ColumnList(this);

			Metadata = new Dictionary<string, object>();
		}

		DbObjectType IDbObjectInfo.ObjectType => DbObjectType.Table;

		ObjectName IDbObjectInfo.FullName => TableName;

		public ObjectName TableName { get; }

		public IDictionary<string, object> Metadata { get; }

		public int TableId { get; set; }

		public string Type { get; }

		public virtual IColumnList Columns { get; }

		public static TableInfo ReadOnly(TableInfo tableInfo) {
			var result = new TableInfo(tableInfo.TableName);
			foreach (var column in tableInfo.Columns) {
				result.Columns.Add(column);
			}

			result.readOnly = true;
			return result;
		}

		public static TableInfo Alias(TableInfo tableInfo, ObjectName alias) {
			var result = new TableInfo(alias);
			foreach (var column in tableInfo.Columns) {
				result.Columns.Add(column);
			}

			return result;
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			TableName.AppendTo(builder);

			builder.AppendLine(" (");
			builder.Indent();

			for (int i = 0; i < Columns.Count; i++) {
				Columns[i].AppendTo(builder);

				if (i < Columns.Count - 1)
					builder.Append(",");

				builder.AppendLine();
			}

			builder.DeIndent();
			builder.Append(")");
		}

		public IndexInfo CreateColumnIndexInfo(int column) {
			var columnName = Columns[column].ColumnName;
			return new IndexInfo(new ObjectName(TableName, $"idx_column[{column}]"), TableName, columnName);
		}

		public override string ToString() {
			return this.ToSqlString();
		}

		public SqlExpression ResolveColumns(SqlExpression expression, bool ignoreCase) {
			var visitor = new ColumnResolverVisitor(this, ignoreCase);
			return visitor.Visit(expression);
		}

		#region ColumnList

		class ColumnList : IColumnList {
			private readonly TableInfo tableInfo;
			private List<ColumnInfo> columns;
			private Dictionary<ObjectName, int> columnsCache;

			public ColumnList(TableInfo tableInfo) {
				this.tableInfo = tableInfo;
				columns = new List<ColumnInfo>();
			}

			public bool IsReadOnly => tableInfo.readOnly;

			public int Count => columns.Count;

			public ColumnInfo this[int index] {
				get => columns[index];
				set {
					AssertNotReadOnly();
					columns[index] = value;
					ClearCache();
				}
			}

			private void AssertNotReadOnly() {
				if (tableInfo.readOnly)
					throw new InvalidOperationException("The table info is read-only");
			}

			private void ClearCache() {
				if (columnsCache != null)
					columnsCache.Clear();
			}

			public void RemoveAt(int index) {
				AssertNotReadOnly();
				columns.RemoveAt(index);
				ClearCache();
			}

			public void Insert(int index, ColumnInfo item) {
				AssertNotReadOnly();

				columns.Insert(index, item);
				ClearCache();
			}

			public bool Remove(ColumnInfo columnInfo) {
				AssertNotReadOnly();

				var index = IndexOf(columnInfo.ColumnName);
				if (index == -1)
					return false;

				RemoveAt(index);
				return true;
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			public IEnumerator<ColumnInfo> GetEnumerator() {
				return columns.GetEnumerator();
			}

			public void Add(ColumnInfo item) {
				AssertNotReadOnly();

				if (IndexOf(item.ColumnName) != -1)
					throw new ArgumentException($"The table {tableInfo.TableName} already defines a column named {item.ColumnName}");

				columns.Add(item);
			}

			public void Clear() {
				AssertNotReadOnly();
				ClearCache();

				columnsCache.Clear();
			}

			public void CopyTo(ColumnInfo[] array, int index) {
				columns.CopyTo(array, index);
			}

			public bool Contains(ColumnInfo item) {
				return IndexOf(item.ColumnName) != -1;
			}

			public int IndexOf(ObjectName columnName) {
				if (columnsCache == null)
					columnsCache = new Dictionary<ObjectName, int>();

				int offset;
				if (!columnsCache.TryGetValue(columnName, out offset)) {
					if (columnName.Parent == null ||
					    !columnName.Parent.Equals(tableInfo.TableName))
						return -1;

					offset = IndexOf(columnName.Name);
					columnsCache[columnName] = offset;
				}

				return offset;
			}

			public int IndexOf(ColumnInfo item) {
				return IndexOf(item.ColumnName);
			}

			public int IndexOf(string columnName) {
				for (int i = 0; i < columns.Count; i++) {
					var columnInfo = columns[i];
					if (String.Equals(columnInfo.ColumnName, columnName, StringComparison.Ordinal))
						return i;
				}

				return -1;
			}

			public ObjectName GetColumnName(int offset) {
				var columnInfo = columns[offset];
				return new ObjectName(tableInfo.TableName, columnInfo.ColumnName);
			}
		}

		#endregion
	}
}