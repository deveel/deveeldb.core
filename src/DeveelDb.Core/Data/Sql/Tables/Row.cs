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
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Services;
using Deveel.Data.Sql.Constraints;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Tables {
	public sealed class Row : IEnumerable<Field> {
		private Dictionary<int, SqlObject> values;

		public Row(ITable table) 
			: this(table, -1) {
		}

		public Row(ITable table, long number) {
			if (table == null)
				throw new ArgumentNullException(nameof(table));

			Table = table;
			RowNumber = number;
			Id = new RowId(TableInfo.TableId, number);
			values = new Dictionary<int, SqlObject>();
		}

		private long RowNumber { get; }

		public ITable Table { get; }

		public RowId Id { get; }

		private TableInfo TableInfo => ((TableInfo) Table.ObjectInfo);

		public SqlObject this[int offset] {
			get => GetValue(offset);
			set => SetValue(offset, value);
		}

		public SqlObject this[string columnName] {
			get => GetValue(columnName);
			set => SetValue(columnName, value);
		}

		public async Task<SqlObject> GetValueAsync(int column) {
			SqlObject value;
			if (column < 0 || column >= TableInfo.Columns.Count)
				throw new ArgumentOutOfRangeException();

			
			if (!values.TryGetValue(column, out value)) {
				value = await Table.GetValueAsync(Id.Number, column);
				values[column] = value;
			}

			return value;
		}

		public SqlObject GetValue(int column) {
			return GetValueAsync(column).Result;
		}

		public Task<SqlObject> GetValueAsync(string columnName) {
			if (String.IsNullOrWhiteSpace(columnName))
				throw new ArgumentNullException(nameof(columnName));

			var offset = TableInfo.Columns.IndexOf(columnName);
			if (offset < 0)
				throw new ArgumentException();

			return GetValueAsync(offset);
		}

		public SqlObject GetValue(string columnName) {
			return GetValueAsync(columnName).Result;
		}

		public void SetValue(int column, SqlObject value) {
			if (column < 0 || column >= TableInfo.Columns.Count)
				throw new ArgumentOutOfRangeException();

			values[column] = value;
		}

		public void SetValue(string columnName, SqlObject value) {
			if (String.IsNullOrWhiteSpace(columnName))
				throw new ArgumentNullException(nameof(columnName));

			var offset = TableInfo.Columns.IndexOf(columnName);
			if (offset < 0)
				throw new ArgumentException();

			SetValue(offset, value);
		}

		public void SetDefault(IContext context, int column) {
			if (column < 0 || column >= TableInfo.Columns.Count)
				throw new ArgumentOutOfRangeException(nameof(column));

			var columnInfo = TableInfo.Columns[column];

			// a NOT NULL constraint check will be done later

			SqlObject value;
			if (columnInfo.HasDefaultValue) {
				value = ReduceDefault(context, columnInfo.DefaultValue);
			} else {
				value = SqlObject.NullOf(columnInfo.ColumnType);
			}

			SetValue(column, value);
		}

		public void SetDefault(IContext context) {
			for (int i = 0; i < TableInfo.Columns.Count; i++) {
				if (!values.ContainsKey(i))
					SetDefault(context, i);
			}
		}

		private SqlObject ReduceDefault(IContext context, SqlExpression expression) {
			var ignoreCase = context.GetValue("ignoreCase", true);

			// Resolve any variables to the table_def for this expression.
			expression = Table.TableInfo.ResolveColumns(expression, ignoreCase);

			using (var rowContext = new Context(context, "Row")) {
				// Get the variable resolver and evaluate over this data.
				var vresolver = new ReferenceResolver(this);
				(rowContext as IContext).Scope.RegisterInstance<IReferenceResolver>(vresolver);

				var reduced = expression.Reduce(rowContext);
				if (reduced.ExpressionType != SqlExpressionType.Constant)
					throw new InvalidOperationException("The DEFAULT expression of the column cannot be reduced to a constant");

				return ((SqlConstantExpression) reduced).Value;
			}
		}

		public IEnumerator<Field> GetEnumerator() {
			return new FieldEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		#region ReferenceResolver

		class ReferenceResolver : IReferenceResolver {
			private readonly Row row;

			public ReferenceResolver(Row row) {
				this.row = row;
			}

			public Task<SqlObject> ResolveReferenceAsync(ObjectName referenceName) {
				if (referenceName.Parent != null &&
				    !row.TableInfo.TableName.Equals(referenceName.Parent))
					return Task.FromResult<SqlObject>(null);

				var columnName = referenceName.Name;
				var index = row.TableInfo.Columns.IndexOf(columnName);
				if (index == -1)
					throw new InvalidOperationException($"Column {columnName} not found in table {row.TableInfo.TableName}");

				SqlObject value;
				if (!row.values.TryGetValue(index, out value))
					throw new InvalidOperationException($"Value of column {columnName} in row was not set");

				return Task.FromResult(value);
			}

			public SqlType ResolveType(ObjectName referenceName) {
				if (referenceName.Parent != null &&
				    !row.TableInfo.TableName.Equals(referenceName.Parent))
					return null;

				var columnName = referenceName.Name;
				var index = row.TableInfo.Columns.IndexOf(columnName);
				if (index == -1)
					throw new InvalidOperationException($"Column {columnName} not found in table {row.TableInfo.TableName}");

				return row.TableInfo.Columns[index].ColumnType;
			}
		}

		#endregion

		#region FieldEnumerator

		class FieldEnumerator : IEnumerator<Field> {
			private readonly Row row;
			private int offset;

			public FieldEnumerator(Row row) {
				this.row = row;
				Reset();
			}

			public void Reset() {
				offset = -1;
			}

			public bool MoveNext() {
				return ++offset < row.TableInfo.Columns.Count;
			}

			object IEnumerator.Current => Current;

			public Field Current => new Field(row, offset);

			public void Dispose() {
				
			}
		}

		#endregion
	}
}