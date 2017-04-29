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
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Query;
using Deveel.Data.Sql.Query.Plan;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlQuantifyExpression : SqlExpression {
		internal SqlQuantifyExpression(SqlExpressionType expressionType, SqlBinaryExpression expression)
			: base(expressionType) {
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			if (!expression.ExpressionType.IsRelational())
				throw new ArgumentException("Cannot quantify a non-relational expression");

			Expression = expression;
		}

		private SqlQuantifyExpression(SerializationInfo info)
			: base(info) {
			Expression = info.GetValue<SqlBinaryExpression>("exp");
		}

		public SqlBinaryExpression Expression { get; }

		public override bool CanReduce => true;

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("exp", Expression);
		}

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitQuantify(this);
		}

		public override SqlType GetSqlType(IContext context) {
			return PrimitiveTypes.Boolean();
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			Expression.Left.AppendTo(builder);

			builder.Append(" ");
			builder.Append(Expression.GetOperatorString());
			builder.AppendFormat(" {0}", ExpressionType.ToString().ToUpperInvariant());

			if (Expression.Right is SqlQueryExpression)
				builder.Append("(");

			Expression.Right.AppendTo(builder);

			if (Expression.Right is SqlQueryExpression)
				builder.Append(")");
		}

		public override Task<SqlExpression> ReduceAsync(IContext context) {
			if (Expression.Right is SqlQueryExpression)
				return ReduceQuery(context);

			var resultType = Expression.Right.GetSqlType(context);
			if (resultType is SqlArrayType) {
				return ReduceArray(context);
			}
			if (resultType is SqlQueryType) {
				return ReduceQuery(context);
			}

			throw new NotSupportedException();
		}

		private async Task<SqlExpression> ReduceQuery(IContext context) {
			var rightResult = await Expression.Right.ReduceAsync(context);
			if (!(rightResult is SqlConstantExpression))
				throw new InvalidOperationException();

			var rightValue = ((SqlConstantExpression)rightResult).Value;
			if (rightValue.IsNull)
				return Constant(SqlObject.Unknown);

			var leftResult = await Expression.Left.ReduceAsync(context);
			if (!(leftResult is SqlConstantExpression))
				throw new NotSupportedException();

			var leftValue = ((SqlConstantExpression)leftResult).Value;

			if (!(rightValue.Type is SqlQueryType))
				throw new SqlExpressionException("Invalid value for a quantification");

			var queryPlan = (IQueryPlanNode) rightValue.Value;

			switch (ExpressionType) {
				case SqlExpressionType.Any:
					return await IsQueryAny(Expression.ExpressionType, leftValue, queryPlan, context);
				case SqlExpressionType.All:
					return await IsQueryAll(Expression.ExpressionType, leftValue, queryPlan, context);
				default:
					throw new NotSupportedException();
			}
		}

		private async Task<SqlExpression> IsQueryAll(SqlExpressionType opType, SqlObject value, IQueryPlanNode query, IContext context) {
			var resolver = context.ResolveService<IReferenceResolver>();

			var correlated = query.DiscoverCorrelatedReferences(1);
			foreach (var expression in correlated) {
				var refValue = await resolver.ResolveReferenceAsync(expression.ReferenceName);
				expression.Value = Constant(refValue);
			}

			var cache = context.ResolveService<ITableCache>();
			if (cache != null)
				cache.Clear();

			var table = await query.ReduceAsync(context);
			if (table.AllMatch(0, opType, value))
				return Constant(SqlObject.Boolean(true));

			return Constant(SqlObject.Boolean(false));
		}

		private async Task<SqlExpression> IsQueryAny(SqlExpressionType opType, SqlObject value, IQueryPlanNode query, IContext context) {
			var resolver = context.ResolveService<IReferenceResolver>();

			var correlated = query.DiscoverCorrelatedReferences(1);
			foreach (var expression in correlated) {
				var refValue = await resolver.ResolveReferenceAsync(expression.ReferenceName);
				expression.Value = Constant(refValue);
			}

			var cache = context.ResolveService<ITableCache>();
			if (cache != null)
				cache.Clear();

			var table = await query.ReduceAsync(context);
			if (table.AnyMatches(0, opType, value))
				return Constant(SqlObject.Boolean(true));

			return Constant(SqlObject.Boolean(false));
		}

		private async Task<SqlExpression> ReduceArray(IContext context) {
			var rightResult = await Expression.Right.ReduceAsync(context);
			if (!(rightResult is SqlConstantExpression))
				throw new InvalidOperationException();

			var rightValue = ((SqlConstantExpression) rightResult).Value;
			if (rightValue.IsNull)
				return Constant(SqlObject.Unknown);

			var leftResult = await Expression.Left.ReduceAsync(context);
			if (!(leftResult is SqlConstantExpression))
				throw new NotSupportedException();

			var leftValue = ((SqlConstantExpression) leftResult).Value;

			if (!(rightValue.Type is SqlArrayType))
				throw new SqlExpressionException("Invalid value for a quantification");

			var array = ((SqlArray) rightValue.Value);

			switch (ExpressionType) {
				case SqlExpressionType.Any:
					return await IsArrayAny(Expression.ExpressionType, leftValue, array, context);
				case SqlExpressionType.All:
					return await IsArrayAll(Expression.ExpressionType, leftValue, array, context);
				default:
					throw new NotSupportedException();
			}
		}

		private SqlObject Relational(SqlExpressionType opType, SqlObject a, SqlObject b) {
			switch (opType) {
				case SqlExpressionType.Equal:
					return a.Equal(b);
				case SqlExpressionType.NotEqual:
					return a.NotEqual(b);
				case SqlExpressionType.GreaterThan:
					return a.GreaterThan(b);
				case SqlExpressionType.LessThan:
					return a.LessThan(b);
				case SqlExpressionType.GreaterThanOrEqual:
					return a.GreaterThanOrEqual(b);
				case SqlExpressionType.LessThanOrEqual:
					return a.LessOrEqualThan(b);
				case SqlExpressionType.Is:
					return a.Is(b);
				case SqlExpressionType.IsNot:
					return a.IsNot(b);
				default:
					return SqlObject.Unknown;
			}
		}

		private static async Task<SqlObject> ItemValue(SqlExpression item, IContext context) {
			var value = await item.ReduceAsync(context);
			if (!(value is SqlConstantExpression))
				return SqlObject.Unknown;

			return ((SqlConstantExpression) value).Value;
		}

		private async Task<SqlExpression> IsArrayAll(SqlExpressionType opType, SqlObject value, SqlArray array, IContext context) {
			foreach (var item in array) {
				var itemValue = await ItemValue(item, context);
				var result = Relational(opType, value, itemValue);
				if (result.IsUnknown)
					return Constant(SqlObject.Unknown);

				if (result.IsFalse)
					return Constant(SqlObject.Boolean(false));
			}

			return Constant(SqlObject.Boolean(true));
		}

		private async Task<SqlExpression> IsArrayAny(SqlExpressionType opType, SqlObject value, SqlArray array, IContext context) {
			foreach (var item in array) {
				var itemValue = await ItemValue(item, context);
				var result = Relational(opType, value, itemValue);
				if (result.IsUnknown)
					return Constant(SqlObject.Unknown);

				if (result.IsTrue)
					return Constant(SqlObject.Boolean(true));
			}

			return Constant(SqlObject.Boolean(false));
		}
	}
}