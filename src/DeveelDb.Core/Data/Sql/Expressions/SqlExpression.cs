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

using Deveel.Data.Sql.Methods;

namespace Deveel.Data.Sql.Expressions {
	public abstract class SqlExpression : ISqlFormattable {
		protected SqlExpression(SqlExpressionType expressionType) {
			ExpressionType = expressionType;
			Precedence = GetPrecedence();
		}

		private int GetPrecedence() {
			switch (ExpressionType) {
				// Group
				case SqlExpressionType.Group:
					return 151;

				// References
				case SqlExpressionType.Reference:
				case SqlExpressionType.Function:
				case SqlExpressionType.VariableAssign:
				case SqlExpressionType.ReferenceAssign:
				case SqlExpressionType.Variable:
				case SqlExpressionType.Parameter:
					return 150;

				// Unary
				case SqlExpressionType.UnaryPlus:
				case SqlExpressionType.Negate:
				case SqlExpressionType.Not:
					return 140;

				// Cast
				case SqlExpressionType.Cast:
					return 139;

				// Multiplicative
				case SqlExpressionType.Multiply:
				case SqlExpressionType.Divide:
				case SqlExpressionType.Modulo:
					return 130;

				// Additive
				case SqlExpressionType.Add:
				case SqlExpressionType.Subtract:
					return 120;

				// Relational
				case SqlExpressionType.GreaterThan:
				case SqlExpressionType.GreaterThanOrEqual:
				case SqlExpressionType.LessThan:
				case SqlExpressionType.LessThanOrEqual:
				case SqlExpressionType.Is:
				case SqlExpressionType.IsNot:
				case SqlExpressionType.Like:
				case SqlExpressionType.NotLike:
					return 110;

				// Equality
				case SqlExpressionType.Equal:
				case SqlExpressionType.NotEqual:
					return 100;
				
				// Logical
				case SqlExpressionType.And:
					return 90;
				case SqlExpressionType.Or:
					return 89;
				case SqlExpressionType.XOr:
					return 88;

				// Conditional
				case SqlExpressionType.Condition:
					return 80;

				// Constant
				case SqlExpressionType.Constant:
					return 70;
			}

			return -1;
		}

		public virtual bool CanReduce {
			get { return false; }
		}

		public SqlExpressionType ExpressionType { get; }

		public SqlType Type => GetSqlType(null);

		public virtual bool IsReference => true;

		internal int Precedence { get; }

		public override string ToString() {
			return this.ToSqlString();
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		protected virtual void AppendTo(SqlStringBuilder builder) {
			
		}

		public virtual SqlExpression Reduce(IContext context) {
			return this;
		}

		public abstract SqlType GetSqlType(IContext context);

		public virtual SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.Visit(this);
		}

		#region Factories

		public static SqlConstantExpression Constant(SqlObject value) {
			return new SqlConstantExpression(value);
		}

		public static SqlBinaryExpression Binary(SqlExpressionType expressionType, SqlExpression left, SqlExpression right) {
			if (!expressionType.IsBinary())
				throw new ArgumentException($"Expression type {expressionType} is not binary");

			return new SqlBinaryExpression(expressionType, left, right);
		}

		public static SqlBinaryExpression Add(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.Add, left, right);

		public static SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.Subtract, left, right);

		public static SqlBinaryExpression Divide(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.Divide, left, right);

		public static SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.Modulo, left, right);

		public static SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.Multiply, left, right);

		public static SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.Equal, left, right);

		public static SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.NotEqual, left, right);

		public static SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.GreaterThan, left, right);

		public static SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.GreaterThanOrEqual, left, right);

		public static SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.LessThan, left, right);

		public static SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.LessThanOrEqual, left, right);

		public static SqlBinaryExpression Is(SqlExpression left, SqlExpression rigth)
			=> Binary(SqlExpressionType.Is, left, rigth);

		public static SqlBinaryExpression And(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.And, left, right);

		public static SqlBinaryExpression Or(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.Or, left, right);

		public static SqlBinaryExpression XOr(SqlExpression left, SqlExpression right)
			=> Binary(SqlExpressionType.XOr, left, right);

		public static SqlUnaryExpression Unary(SqlExpressionType expressionType, SqlExpression operand) {
			if (!expressionType.IsUnary())
				throw new ArgumentException($"Expression type {expressionType} is not unary");

			return new SqlUnaryExpression(expressionType, operand);
		}

		public static SqlUnaryExpression Not(SqlExpression operand) {
			return Unary(SqlExpressionType.Not, operand);
		}

		public static SqlUnaryExpression Negate(SqlExpression operand) {
			return Unary(SqlExpressionType.Negate, operand);
		}

		public static SqlUnaryExpression Plus(SqlExpression operand) {
			return new SqlUnaryExpression(SqlExpressionType.UnaryPlus, operand);
		}

		public static SqlCastExpression Cast(SqlExpression value, SqlType targetType) {
			return new SqlCastExpression(value, targetType);
		}

		public static SqlReferenceExpression Reference(ObjectName reference) {
			return new SqlReferenceExpression(reference);
		}

		public static SqlVariableExpression Variable(string name) {
			return new SqlVariableExpression(name);
		}

		public static SqlVariableAssignExpression VariableAssign(string name, SqlExpression value) {
			return new SqlVariableAssignExpression(name, value);
		}

		public static SqlReferenceAssignExpression ReferenceAssign(ObjectName referenceName, SqlExpression value) {
			return new SqlReferenceAssignExpression(referenceName, value);
		}

		public static SqlConditionExpression Condition(SqlExpression test, SqlExpression ifTrue, SqlExpression ifFalse)
			=> new SqlConditionExpression(test, ifTrue, ifFalse);

		public static SqlParameterExpression Parameter() => new SqlParameterExpression();

		public static SqlGroupExpression Group(SqlExpression expression)
			=> new SqlGroupExpression(expression);

		public static SqlQuantifyExpression Quantify(SqlExpressionType expressionType, SqlBinaryExpression expression) {
			if (!expressionType.IsQuantify())
				throw new ArgumentException($"The expression type {expressionType} is not a quantification expression");

			return new SqlQuantifyExpression(expressionType, expression);
		}

		public static SqlQuantifyExpression Any(SqlBinaryExpression expression)
			=> Quantify(SqlExpressionType.Any, expression);

		public static SqlQuantifyExpression All(SqlBinaryExpression expression)
			=> Quantify(SqlExpressionType.All, expression);

		public static SqlFunctionExpression Function(ObjectName functionName, params InvokeArgument[] args) {
			return new SqlFunctionExpression(functionName, args);
		}

		#endregion
	}
}