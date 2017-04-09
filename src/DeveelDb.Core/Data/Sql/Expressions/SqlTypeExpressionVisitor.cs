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

using Deveel.Data.Configuration;
using Deveel.Data.Services;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Expressions {
	class SqlTypeExpressionVisitor : SqlExpressionVisitor {
		private readonly IContext context;

		public SqlTypeExpressionVisitor(IContext context) {
			this.context = context;
		}

		public SqlType Type { get; private set; }

		public override SqlExpression VisitConstant(SqlConstantExpression expression) {
			Type = expression.Value.Type;
			return base.VisitConstant(expression);
		}

		public override SqlExpression VisitCast(SqlCastExpression expression) {
			Type = expression.TargetType;
			return base.VisitCast(expression);
		}

		public override SqlExpression VisitVariable(SqlVariableExpression expression) {
			var ignoreCase = context.GetValue("ignoreCase", true);
			var variable = context.ResolveVariable(expression.VariableName, ignoreCase);
			if (variable != null)
				Type = variable.Type;

			return base.VisitVariable(expression);
		}

		public override SqlExpression VisitVariableAssign(SqlVariableAssignExpression expression) {
			var variable = context.ResolveVariable(expression.VariableName, true);
			if (variable != null)
				Type = variable.Type;

			return base.VisitVariableAssign(expression);
		}

		public override SqlExpression VisitBinary(SqlBinaryExpression expression) {
			switch (expression.ExpressionType) {
				case SqlExpressionType.Add:
				case SqlExpressionType.Subtract:
				case SqlExpressionType.Multiply:
				case SqlExpressionType.Modulo: {
					var leftType = expression.Left.ReturnType(context);
					var rightType = expression.Right.ReturnType(context);
					Type = leftType.Wider(rightType);
					break;
				}
				default:
					Type = PrimitiveTypes.Boolean();
					break;
			}
			return base.VisitBinary(expression);
		}

		public override SqlExpression VisitReference(SqlReferenceExpression expression) {
			var resolver = context.Scope.Resolve<IReferenceResolver>();
			var reference = resolver.ResolveReference(expression.ReferenceName);
			if (reference != null)
				Type = reference.Type;

			return base.VisitReference(expression);
		}
	}
}