using System;

using Deveel.Data.Query;
using Deveel.Data.Sql.Methods;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlExpressionVisitorTests {
		[Fact]
		public static void VisitConstant() {
			var exp = SqlExpression.Constant(SqlObject.Boolean(false));
			Visit(exp);
		}

		[Fact]
		public static void VisitBinary() {
			var exp = SqlExpression.Binary(SqlExpressionType.Add,
				SqlExpression.Constant(SqlObject.Integer(22)),
				SqlExpression.Constant(SqlObject.Integer(344)));
			Visit(exp);
		}

		[Fact]
		public static void VisitVariable() {
			var exp = SqlExpression.Variable("a");
			Visit(exp, true);
		}

		[Fact]
		public static void VisitVariableAssign() {
			var exp = SqlExpression.VariableAssign("a", SqlExpression.Constant(SqlObject.Integer(674)));
			Visit(exp);
		}

		[Fact]
		public static void VisitGroup() {
			var exp = SqlExpression.Group(SqlExpression.Constant(SqlObject.BigInt(345)));
			Visit(exp);
		}

		[Fact]
		public static void VisitUnary() {
			var exp = SqlExpression.Unary(SqlExpressionType.UnaryPlus, SqlExpression.Constant(SqlObject.Integer(33)));
			Visit(exp);
		}

		[Fact]
		public static void VisitReference() {
			var exp = SqlExpression.Reference(new ObjectName("a"));
			Visit(exp, true);
		}

		[Fact]
		public static void VisitReferenceAssign() {
			var exp = SqlExpression.ReferenceAssign(new ObjectName("a"), SqlExpression.Constant(SqlObject.Integer(33)));
			Visit(exp);
		}

		[Fact]
		public static void VisitCast() {
			var exp = SqlExpression.Cast(SqlExpression.Constant(SqlObject.String(new SqlString("223"))),
				PrimitiveTypes.BigInt());
			Visit(exp);
		}

		[Fact]
		public static void VisitCondition() {
			var exp = SqlExpression.Condition(SqlExpression.Constant(SqlObject.Boolean(true)),
				SqlExpression.Constant(SqlObject.BigInt(902)),
				SqlExpression.Constant(SqlObject.Integer(433)));
			Visit(exp);
		}

		[Fact]
		public static void VisitParameter() {
			var exp = SqlExpression.Parameter();
			Visit(exp, true);
		}

		[Fact]
		public static void VisitQuery() {
			var exp = new SqlQueryExpression();
			exp.Items.Add(SqlExpression.Reference(new ObjectName("a")));
			exp.From.Table(new ObjectName("b"));
			exp.From.Join(JoinType.Left,
				SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("b.id")), SqlExpression.Reference(ObjectName.Parse("c.b_id"))));
			exp.From.Table(new ObjectName("c"));
			exp.Where = SqlExpression.GreaterThanOrEqual(SqlExpression.Reference(new ObjectName("a")), SqlExpression.Constant(SqlObject.BigInt(22)));

			Visit(exp);
		}

		[Fact]
		public static void VisitQuantify() {
			var exp = SqlExpression.Quantify(SqlExpressionType.All, 
				SqlExpression.Equal(SqlExpression.Constant(SqlObject.BigInt(43)), 
				SqlExpression.Constant(SqlObject.Array(SqlObject.BigInt(33), SqlObject.Integer(1222)))));

			Visit(exp);
		}


		[Fact]
		public static void VisitFunction() {
			var exp = SqlExpression.Function(ObjectName.Parse("sys.func1"),
				new[] {new InvokeArgument("a", SqlObject.Integer(3))});

			Visit(exp);
		}

		private static void Visit(SqlExpression exp, bool equals = false) {
			var visitor = new SqlExpressionVisitor();
			var result = exp.Accept(visitor);
			Assert.IsType(exp.GetType(), result);
			Assert.Equal(equals, result.Equals(exp));

			result = visitor.Visit(exp);
			Assert.IsType(exp.GetType(), result);
			Assert.Equal(equals, result.Equals(exp));
		}
	}
}