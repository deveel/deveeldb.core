using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Tables {
	public class FunctionTable : DataTableBase {
		private readonly ITable table;
		private int uniqueId;
		private readonly FunctionColumnInfo[] columns;

		private static int uniqueKeySeq = 0;
		private static readonly ObjectName FunctionTableName = new ObjectName("#FUNCTION_TABLE#");

		public FunctionTable(IContext context, ITable table, FunctionColumnInfo[] columns) {
			// Make sure we are synchronized over the class.
			lock (typeof(FunctionTable)) {
				uniqueId = uniqueKeySeq;
				++uniqueKeySeq;
			}

			uniqueId = (uniqueId & 0x0FFFFFFF) | 0x010000000;

			var tableInfo = new TableInfo(FunctionTableName);

			for (int i = 0; i < columns.Length; i++) {
				tableInfo.Columns.Add(columns[i].ColumnInfo);
			}

			TableInfo = tableInfo;
			this.columns = columns;

			this.table = table;
			RowCount = table.RowCount;

			Context = new Context(context, "#FUNCTION#");
		}

		public override TableInfo TableInfo { get; }

		public override long RowCount { get; }

		protected IContext Context { get; }

		private ITableCache Cache => Context.ResolveService<ITableCache>();

		protected virtual void PrepareRowContext(IContext context, long row) {
			context.RegisterInstance<IReferenceResolver>(new RowReferenceResolver(table, row));
		}

		public override SqlObject GetValue(long row, int column) {
			var cache = Cache;
			var expr = columns[column];

			SqlObject value;

			if (cache != null && !expr.IsReduced) {
				var fieldId = new FieldId(new RowId(TableInfo.TableId, row), column);
				if (!cache.TryGetValue(fieldId, out value)) {
					value = GetValueDirect(expr.Function, row);
				}
			} else {
				value = GetValueDirect(expr.Function, row);
			}

			return value;
		}

		private SqlObject GetValueDirect(SqlExpression expression, long row) {
			SqlExpression result;

			using (var context = Context.Create($"#FUNCTION#({row})")) {
				PrepareRowContext(context, row);

				result = expression.Reduce(context);
			}

			if (result.ExpressionType != SqlExpressionType.Constant)
				throw new ArgumentException();

			return ((SqlConstantExpression)result).Value;
		}

		public virtual VirtualTable GroupMax(ObjectName maxColumn) {
			BigList<long> rowList;

			if (table.RowCount <= 0) {
				rowList = new BigList<long>(0);
			} else {
				// OPTIMIZATION: This should be optimized.  It should be fairly trivial
				//   to generate a Table implementation that efficiently merges this
				//   function table with the reference table.

				// This means there is no grouping, so merge with entire table,
				var rowCount = table.RowCount;
				rowList = new BigList<long>(rowCount);
				using (var en = table.GetEnumerator()) {
					while (en.MoveNext()) {
						rowList.Add(en.Current.Id.Number);
					}
				}
			}

			// Create a virtual table that's the new group table merged with the
			// functions in this...

			var tabs = new[] { table, this };
			var rowSets = new IEnumerable<long>[] { rowList, rowList };

			return new VirtualTable(tabs, rowSets);
		}

		public override IEnumerator<Row> GetEnumerator() {
			return new SimpleRowEnumerator(this);
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (Context != null)
					Context.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}