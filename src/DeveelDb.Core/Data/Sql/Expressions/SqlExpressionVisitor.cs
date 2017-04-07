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

namespace Deveel.Data.Sql.Expressions {
	public class SqlExpressionVisitor : ISqlExpressionVisitor {
		public virtual SqlExpression Visit(SqlExpression expression) {
			if (expression == null)
				return null;

			switch (expression.ExpressionType) {
				case SqlExpressionType.Add:
				case SqlExpressionType.Subtract:
				case SqlExpressionType.Divide:
				case SqlExpressionType.Multiply:
				case SqlExpressionType.Modulo:
				case SqlExpressionType.Is:
				case SqlExpressionType.IsNot:
				case SqlExpressionType.Equal:
				case SqlExpressionType.NotEqual:
				case SqlExpressionType.GreaterThan:
				case SqlExpressionType.GreaterThanOrEqual:
				case SqlExpressionType.LessThan:
				case SqlExpressionType.LessThanOrEqual:
				case SqlExpressionType.And:
				case SqlExpressionType.Or:
				case SqlExpressionType.XOr:
					return VisitBinary((SqlBinaryExpression) expression);
				case SqlExpressionType.Not:
				case SqlExpressionType.Negate:
				case SqlExpressionType.UnaryPlus:
					return VisitUnary((SqlUnaryExpression) expression);
				case SqlExpressionType.Cast:
					return VisitCast((SqlCastExpression) expression);
				case SqlExpressionType.Reference:
					return VisitReference((SqlReferenceExpression) expression);
				case SqlExpressionType.Constant:
					return VisitConstant((SqlConstantExpression) expression);
				default:
					throw new SqlExpressionException($"Invalid expression type: {expression.ExpressionType}");
			}
		}

		public virtual SqlExpression VisitReference(SqlReferenceExpression expression) {
			return expression;
		}

		public virtual SqlExpression VisitCast(SqlCastExpression expression) {
			var value = expression.Value;
			if (value != null)
				value = Visit(value);

			return SqlExpression.Cast(value, expression.TargetType);
		}

		public virtual SqlExpression VisitBinary(SqlBinaryExpression expression) {
			var left = expression.Left;
			var right = expression.Right;
			if (left != null)
				left = Visit(left);
			if (right != null)
				right = Visit(right);

			return SqlExpression.Binary(expression.ExpressionType, left, right);
		}

		public virtual SqlExpression VisitUnary(SqlUnaryExpression expression) {
			var operand = expression.Operand;
			if (operand != null)
				operand = Visit(operand);

			return SqlExpression.Unary(expression.ExpressionType, operand);
		}

		public virtual SqlExpression VisitConstant(SqlConstantExpression constant) {
			return SqlExpression.Constant(constant.Value);
		}
	}
}