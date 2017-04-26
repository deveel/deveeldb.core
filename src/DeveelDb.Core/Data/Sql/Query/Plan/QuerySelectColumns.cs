using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Query.Plan {
	class QuerySelectColumns {
		/// <summary>
		/// The tables we are selecting from.
		/// </summary>
		private readonly QueryExpressionFrom fromSet;

		private readonly List<SelectColumn> selectedColumns;

		// The count of aggregate and constant columns included in the result set.
		// Aggregate columns are, (count(*), avg(cost_of) * 0.75, etc).  Constant
		// columns are, (9 * 4, 2, (9 * 7 / 4) + 4, etc).

		public QuerySelectColumns(QueryExpressionFrom fromSet) {
			this.fromSet = fromSet;
			selectedColumns = new List<SelectColumn>();
		}

		public IEnumerable<SelectColumn> SelectedColumns => selectedColumns;

		public void SelectSingleColumn(SqlQueryExpressionItem col) {
			selectedColumns.Add(new SelectColumn {
				Expression = col.Expression,
				Alias = col.Alias
			});
		}

		private void AddAllFromTable(IFromTable table) {
			// Select all the tables
			var columns = table.Columns;
			foreach (ObjectName name in columns) {
				// Make up the SelectColumn
				var e = SqlExpression.Reference(name);
				var column = new SelectColumn {
					Expression = e,
					ResolvedName = name,
					InternalName = name
				};

				// Add to the list of columns selected
				selectedColumns.Add(column);
			}
		}

		public void SelectAllColumnsFromSource(ObjectName tableName) {
			// Attempt to find the table in the from set.
			string schema = null;
			if (tableName.Parent != null)
				schema = tableName.Parent.Name;

			var table = fromSet.FindTable(schema, tableName.Name);
			if (table == null)
				throw new InvalidOperationException(tableName + ".* is not a valid reference.");

			AddAllFromTable(table);
		}

		public void SelectAllColumnsFromAllSources() {
			for (int p = 0; p < fromSet.SourceCount; ++p) {
				var table = fromSet.GetTableSource(p);
				AddAllFromTable(table);
			}
		}

		public SelectColumn PrepareColumn(SelectColumn column,
			IContext context,
			IList<SelectColumn> functionColumns,
			ref int aggregateCount) {
			if (column.Expression is SqlQueryExpression)
				throw new InvalidOperationException("Sub-query expressions are invalid in select columns.");

			SelectColumn newColumn;

			var exp = column.Expression;
			if (exp != null)
				exp = exp.Prepare(fromSet.ExpressionPreparer);

			if (exp is SqlReferenceExpression) {
				var sqlRef = (SqlReferenceExpression) exp;
				var colName = sqlRef.ReferenceName;
				ObjectName resolvedName = null;

				var alias = column.Alias;
				if (String.IsNullOrEmpty(alias)) {
					resolvedName = colName;
				} else {
					resolvedName = new ObjectName(alias);
				}

				newColumn = new SelectColumn {
					Expression = exp,
					Alias = alias,
					InternalName = colName,
					ResolvedName = resolvedName
				};
			} else {
				var funcAlias = functionColumns.Count.ToString(CultureInfo.InvariantCulture);
				if (column.Expression.HasAggregate(context)) {
					aggregateCount++;
					funcAlias += "_A";
				}

				var alias = column.Alias;
				if (string.IsNullOrEmpty(alias))
					alias = exp.ToString();

				newColumn = new SelectColumn {
					Expression = exp,
					Alias = alias,
					InternalName = new ObjectName(FunctionTable.Name, funcAlias),
					ResolvedName = ObjectName.Parse(alias)
				};

				functionColumns.Add(newColumn);
			}

			return newColumn;
		}
	}
}