using System;
using System.Threading.Tasks;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Indexes;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class RangeSelectNode : SingleQueryPlanNode {
		public RangeSelectNode(IQueryPlanNode child, SqlExpression expression)
			: base(child) {
			Expression = expression;
		}

		public SqlExpression Expression { get; }

		public override async Task<ITable> ReduceAsync(IContext context) {
			var table = await Child.ReduceAsync(context);

			var exp = Expression;

			// Assert that all variables in the expression are identical.
			var columnNames = exp.DiscoverReferences();
			ObjectName columnName = null;
			foreach (var cv in columnNames) {
				if (columnName != null && !cv.Equals(columnName))
					throw new InvalidOperationException("Range plan does not contain common column.");

				columnName = cv;
			}

			// Find the variable field in the table.
			var col = table.TableInfo.Columns.IndexOf(columnName);
			if (col == -1)
				throw new InvalidOperationException("Could not find column reference in table: " + columnName);

			var field = table.TableInfo.Columns[col];

			// Calculate the range
			var range = new IndexRangeSet();
			var calculator = new RangeSetCalculator(context, field, range);
			range = calculator.Calculate(exp);

			// Select the range from the table
			var ranges = range.ToArray();
			return table.SelectRange(columnName, ranges);
		}

		#region RangeSetUpdater

		class RangeSetUpdater : SqlExpressionVisitor {
			private IndexRangeSet indexRangeSet;
			private readonly IContext context;
			private readonly ColumnInfo field;

			public RangeSetUpdater(IContext context, ColumnInfo field, IndexRangeSet indexRangeSet) {
				this.context = context;
				this.field = field;
				this.indexRangeSet = indexRangeSet;
			}

			public IndexRangeSet Update(SqlExpression expression) {
				Visit(expression);
				return indexRangeSet;
			}

			public override SqlExpression VisitBinary(SqlBinaryExpression binaryEpression) {
				var op = binaryEpression.ExpressionType;

				// Evaluate to an object
				var value = ((SqlConstantExpression) binaryEpression.Right.Reduce(context)).Value;

				// If the evaluated object is not of a comparable type, then it becomes
				// null.
				var fieldType = field.ColumnType;
				if (!value.Type.IsComparable(fieldType))
					value = SqlObject.NullOf(fieldType);

				// Intersect this in the range set
				indexRangeSet = indexRangeSet.Intersect(op, new IndexKey(value));

				return base.VisitBinary(binaryEpression);
			}
		}

		#endregion

		#region RangeSetCalculator

		class RangeSetCalculator : SqlExpressionVisitor {
			private IndexRangeSet rangeSet;
			private readonly IContext context;
			private readonly ColumnInfo field;

			public RangeSetCalculator(IContext context, ColumnInfo field, IndexRangeSet rangeSet) {
				this.context = context;
				this.field = field;
				this.rangeSet = rangeSet;
			}

			private IndexRangeSet UpdateRange(SqlExpression expression) {
				var updater = new RangeSetUpdater(context, field, rangeSet);
				return updater.Update(expression);
			}

			private IndexRangeSet CalcExpression(SqlExpression expression) {
				var indexRangeSet = new IndexRangeSet();
				var calculator = new RangeSetCalculator(context, field, indexRangeSet);
				return calculator.Calculate(expression);
			}

			public override SqlExpression VisitBinary(SqlBinaryExpression binaryEpression) {
				if (binaryEpression.ExpressionType == SqlExpressionType.And) {
					rangeSet = UpdateRange(binaryEpression.Left);
					rangeSet = UpdateRange(binaryEpression.Right);
				} else if (binaryEpression.ExpressionType == SqlExpressionType.Or) {
					var left = CalcExpression(binaryEpression.Left);
					var right = CalcExpression(binaryEpression.Right);

					rangeSet = rangeSet.Union(left);
					rangeSet = rangeSet.Union(right);
				} else {
					rangeSet = UpdateRange(binaryEpression);
				}

				return base.VisitBinary(binaryEpression);
			}

			public IndexRangeSet Calculate(SqlExpression expression) {
				Visit(expression);
				return rangeSet;
			}
		}

		#endregion
	}
}