using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Tables {
	public class FunctionTable : DataTableBase {
		private int uniqueId;
		private readonly byte[] expInfo;
		private readonly SqlExpression[] expList;

		private static int uniqueKeySeq = 0;
		private static readonly ObjectName FunctionTableName = new ObjectName("#FUNCTION_TABLE#");

		private FunctionTable(IContext context, ITable table) {
			// Make sure we are synchronized over the class.
			lock (typeof(FunctionTable)) {
				uniqueId = uniqueKeySeq;
				++uniqueKeySeq;
			}

			uniqueId = (uniqueId & 0x0FFFFFFF) | 0x010000000;

			Table = table;
			RowCount = table.RowCount;
			Context = new TableContext(context);

			var refResolver = new RowReferenceResolver(table, 0);
			Context.Scope.Register<IReferenceResolver>(refResolver);
		}

		protected FunctionTable(IContext context, FunctionTable parent)
			: this(context, parent.Table) {
			TableInfo = parent.TableInfo;
			expInfo = (byte[]) parent.expInfo.Clone();
			expList = (SqlExpression[]) parent.expList.Clone();
		}

		public FunctionTable(IContext context, ITable table, SqlExpression[] functionList, string[] columnNames)
			: this(context, table) {
			var tableInfo = new TableInfo(FunctionTableName);

			expList = new SqlExpression[functionList.Length];
			expInfo = new byte[functionList.Length];

			// Create a new DataColumnInfo for each expression, and work out if the
			// expression is simple or not.
			for (int i = 0; i < functionList.Length; ++i) {
				var expr = functionList[i];
				// Examine the expression and determine if it is simple or not
				if (!expr.IsConstant() && !expr.HasAggregate(Context)) {
					// If expression is a constant, solve it
					var result = expr.Reduce(Context);
					if (result.ExpressionType != SqlExpressionType.Constant)
						throw new InvalidOperationException();

					expr = result;
					expList[i] = expr;
					expInfo[i] = 1;
				} else {
					// Otherwise must be dynamic
					expList[i] = expr;
					expInfo[i] = 0;
				}

				// Make the column info
				tableInfo.Columns.Add(new ColumnInfo(columnNames[i], expr.GetSqlType(Context)));
			}

			TableInfo = tableInfo;
		}

		protected internal ITable Table { get; }

		public override TableInfo TableInfo { get; }

		public override long RowCount { get; }

		protected IContext Context { get; }

		protected virtual void PrepareRowContext(IContext context, long row) {
			context.Scope.RegisterInstance<IReferenceResolver>(new RowReferenceResolver(Table, row));
		}

		public override SqlObject GetValue(long row, int column) {
			// TODO: implement caching!

			var expr = expList[column];

			SqlExpression exp;

			using (var context = new RowContext(Context, row)) {
				PrepareRowContext(context, row);

				exp = expr.Reduce(context);
			}

			if (exp.ExpressionType != SqlExpressionType.Constant)
				throw new ArgumentException();

			var value = ((SqlConstantExpression)exp).Value;

			// TODO: implement cache!

			return value;
		}

		public GroupTable GroupBy(ObjectName[] columns) {
			return new GroupTable(Context, this, columns);
		}

		public virtual VirtualTable GroupMax(ObjectName maxColumn) {
			var table = Table;
			IList<long> rowList;

			if (table.RowCount <= 0) {
				rowList = new List<long>(0);
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

		#region TableContext

		class TableContext : Context {
			public TableContext(IContext context)
				: base(context) {	
			}

			protected override string ContextName => "#FUNCTION#";
		}

		#endregion

		#region RowContext

		class RowContext : Context {
			private readonly long row;

			public RowContext(IContext parent, long row)
				: base(parent) {
				this.row = row;
			}

			protected override string ContextName => $"#FUNCTION#({row})";
		}

		#endregion
	}
}