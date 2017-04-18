using System;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlExpressionPrepareTestst {
		[Fact]
		public static void PrepareFromSqlQuery() {
			var sqlQuery = new SqlQuery("SELECT * FROM table1 WHERE a = ? AND b = ?", SqlQueryParameterNaming.Marker);
			sqlQuery.Parameters.Add(new SqlQueryParameter(PrimitiveTypes.Integer(), (SqlNumber)2));
			sqlQuery.Parameters.Add(new SqlQueryParameter(PrimitiveTypes.Integer(), (SqlNumber)1023));

			var preparer = sqlQuery.ExpressionPreparer;

			var queryExp = new SqlQueryExpression();
			queryExp.From.Table(new ObjectName("table1"));
			queryExp.Items.Add(SqlExpression.Reference(new ObjectName("*")));
			queryExp.Where = SqlExpression.And(
				SqlExpression.Equal(SqlExpression.Reference(new ObjectName("a")), SqlExpression.Parameter()),
				SqlExpression.Equal(SqlExpression.Reference(new ObjectName("b")), SqlExpression.Parameter()));

			var prepared = queryExp.Prepare(preparer);

			Assert.IsType<SqlQueryExpression>(prepared);

			var sb = new SqlStringBuilder();
			sb.AppendLine("SELECT *");
			sb.AppendLine("  FROM table1");
			sb.Append("  WHERE a = 2 AND b = 1023");

			var expectString = sb.ToString();
			Assert.Equal(expectString, prepared.ToString());
		}
	}
}