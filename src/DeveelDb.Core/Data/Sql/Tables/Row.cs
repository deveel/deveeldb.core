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

using Deveel.Data.Services;
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

		public IEnumerator<Field> GetEnumerator() {
			return new FieldEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public IReferenceResolver GetResolver() {
			return new RowReferenceResolver(Table, RowNumber);
		}

		public async Task<SqlExpression> ReduceExpressionAsync(IContext context, SqlExpression expression) {
			using (var rowContext = context.Create($"row_{Id}", scope => {
				scope.AddReferenceResolver(GetResolver());
			})) {
				return await expression.ReduceAsync(rowContext);
			}
		}

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