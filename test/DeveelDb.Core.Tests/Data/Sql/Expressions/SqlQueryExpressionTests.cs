using System;

using Deveel.Data.Query;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlQueryExpressionTests {
		[Theory]
		[InlineData("a.*", null, "a.*")]
		[InlineData("a", "b", "a AS b")]
		public static void CreateNewTableSource(string tableName, string alias, string expected) {
			var name = ObjectName.Parse(tableName);
			var source = new SqlQueryExpressionSource(name, alias);

			Assert.True(source.IsTable);
			Assert.False(source.IsQuery);
			Assert.Equal(!String.IsNullOrWhiteSpace(alias), source.IsAliased);
			Assert.Equal(expected, source.ToString());
		}

		[Theory]
		[InlineData("a", null, "(SELECT * FROM a)")]
		[InlineData("a", "b", "(SELECT * FROM a) AS b")]
		public static void CreateNewQuerySource(string fromTableName, string alias, string expected) {
			var fromTable = ObjectName.Parse(fromTableName);
			var query = new SqlQueryExpression();
			query.All = true;
			query.From.Table(fromTable);
			var source = new SqlQueryExpressionSource(query, alias);
			
			Assert.True(source.IsQuery);
			Assert.False(source.IsTable);
			Assert.Equal(!String.IsNullOrWhiteSpace(alias), source.IsAliased);
			Assert.Equal(alias, source.Alias);
			Assert.True(source.Query.All);
			Assert.Equal(expected, source.ToString());
		}

		[Theory]
		[InlineData("a", "b", "table1", "SELECT a, b FROM table1")]
		[InlineData("a.*", "b", "a", "SELECT a.*, b FROM a")]
		public static void CreateNewSimpleQuery(string item1, string item2, string tableName, string expected) {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse(item1)));
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse(item2)));
			query.From.Table(ObjectName.Parse(tableName));

			Assert.False(query.From.IsEmpty);
			Assert.Equal(expected, query.ToString());
		}

		[Fact]
		public static void MakeNewNaturalJoinQuery() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("a.*")));
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("b.*")));
			query.From.Table(ObjectName.Parse("table1"), "a");
			query.From.Table(ObjectName.Parse("table2"), "b");

			const string expected = "SELECT a.*, b.* FROM table1 AS a, table2 AS b";
			Assert.Equal(expected, query.ToString());
		}

		[Fact]
		public static void MakeNewInnerJoinQuery() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("a.*")));
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("b.*")));
			query.From.Table(ObjectName.Parse("table1"), "a");
			query.From.Join(JoinType.Inner,
				SqlExpression.Equal(SqlExpression.Reference(ObjectName.Parse("a.id")),
					SqlExpression.Reference(ObjectName.Parse("b.a_id"))));
			query.From.Table(ObjectName.Parse("table2"), "b");

			const string expected = "SELECT a.*, b.* FROM table1 AS a INNER JOIN table2 AS b ON a.id = b.a_id";
			Assert.Equal(expected, query.ToString());
		}
	}
}