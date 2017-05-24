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
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query.Plan {
	class FromTableDirect : IFromTable {
		private readonly bool ignoreCase;
		private readonly ITableQueryInfo tableQueryInfo;
		private readonly TableInfo tableInfo;

		public FromTableDirect(bool ignoreCase, ITableQueryInfo tableQueryInfo, string uniqueName, ObjectName givenName, ObjectName rootName) {
			this.ignoreCase = ignoreCase;
			this.tableInfo = tableQueryInfo.TableInfo;
			UniqueName = uniqueName;
			this.tableQueryInfo = tableQueryInfo;

			if (givenName != null) {
				TableName = givenName;
			} else {
				TableName = rootName;
			}

			RootName = rootName;
		}

		public string UniqueName { get; }

		public ObjectName TableName { get; }

		public ObjectName RootName { get; }

		public Task<IQueryPlanNode> GetQueryPlanAsync() {
			return tableQueryInfo.GetQueryPlanAsync();
		}

		private bool StringCompare(string str1, string str2) {
			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			return String.Equals(str1, str2, comparison);
		}

		public ObjectName[] Columns {
			get {
				int colCount = tableInfo.Columns.Count;
				var vars = new ObjectName[colCount];
				for (int i = 0; i < colCount; ++i) {
					vars[i] = new ObjectName(TableName, tableInfo.Columns[i].ColumnName);
				}
				return vars;
			}
		}

		public bool MatchesReference(string catalog, string schema, string table) {
			var schemaName = TableName.Parent;
			var catalogName = schemaName?.Parent;

			// Does this table name represent the correct schema?
			var givenSchema = schemaName?.Name;
			if (schema != null && !StringCompare(schema, givenSchema)) {
				// If schema is present and we can't resolve to this schema then false
				return false;
			}

			var givenCatalog = catalogName?.Name;
			if (catalog != null && !StringCompare(catalog, givenCatalog))
				return false;

			if (table != null && !StringCompare(table, TableName.Name)) {
				// If table name is present and we can't resolve to this table name
				// then return false
				return false;
			}

			// Match was successful,
			return true;

		}

		public int ResolveColumnCount(string catalog, string schema, string table, string column) {
			// NOTE: With this type, we can only ever return either 1 or 0 because
			//   it's impossible to have an ambiguous reference

			var schemaName = TableName.Parent;
			var catalogName = schemaName?.Parent;

			var givenCatalog = catalogName?.Name;
			if (catalog != null && !StringCompare(catalog, givenCatalog))
				return 0;

			var givenSchema = schemaName?.Name;
			if (schema != null && !StringCompare(schema, givenSchema))
				return 0;

			if (table != null && !StringCompare(table, TableName.Name)) {
				return 0;
			}

			if (column != null) {
				// TODO: the case-insensitive search in TableInfo
				if (!ignoreCase) {
					// Can we resolve the column in this table?
					int i = tableInfo.Columns.IndexOf(column);

					// If i doesn't equal -1 then we've found our column
					return i == -1 ? 0 : 1;
				}

				return tableInfo.Columns.Count(columnInfo => StringCompare(columnInfo.ColumnName, column));
			}

			// Return the column count
			return tableInfo.Columns.Count;
		}

		public ObjectName ResolveColumn(string catalog, string schema, string table, string column) {
			var schemaName = TableName.Parent;
			var catalogName = schemaName == null ? null : schemaName.Parent;

			var givenCatalog = catalogName != null ? catalogName.Name : null;
			if (catalog != null && !StringCompare(catalog, givenCatalog))
				throw new InvalidOperationException("Incorrect catalog.");

			// Does this table name represent the correct schema?
			var givenSchema = TableName.Parent != null ? TableName.Parent.Name : null;
			if (schema != null && !StringCompare(schema, givenSchema))
				// If schema is present and we can't resolve to this schema
				throw new InvalidOperationException("Incorrect schema.");

			if (table != null && !StringCompare(table, TableName.Name))
				// If table name is present and we can't resolve to this table name
				throw new InvalidOperationException("Incorrect table.");

			if (column != null) {
				if (!ignoreCase) {
					// Can we resolve the column in this table?
					int i = tableInfo.Columns.IndexOf(column);
					if (i == -1)
						throw new InvalidOperationException("Could not resolve '" + column + "'");

					return new ObjectName(TableName, column);
				}

				// Case insensitive search (this is slower than case sensitive).
				var columnName =
					tableInfo.Columns.Where(x => StringCompare(x.ColumnName, column))
						.Select(x => x.ColumnName)
						.FirstOrDefault();

				if (String.IsNullOrEmpty(columnName))
					throw new InvalidOperationException($"Could not resolve column '{column}' within the table '{TableName}'.");

				return new ObjectName(TableName, columnName);
			}

			// Return the first column in the table
			return new ObjectName(TableName, tableInfo.Columns[0].ColumnName);
		}
	}
}