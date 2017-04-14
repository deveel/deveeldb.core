﻿using System;

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

		[Fact]
		public static void CreateNewQuerySource() {
			var fromTable = ObjectName.Parse("table1");
			var query = new SqlQueryExpression();
			query.All = true;
			query.From.Table(fromTable);
			var source = new SqlQueryExpressionSource(query, "a");
			
			Assert.True(source.IsQuery);
			Assert.False(source.IsTable);
			Assert.True(source.IsAliased);
			Assert.Equal("a", source.Alias);
			Assert.True(source.Query.All);

			var expected = new SqlStringBuilder();
			expected.AppendLine("(SELECT *");
			expected.Append("  FROM table1) AS a");

			Assert.Equal(expected.ToString(), source.ToString());
		}

		[Fact]
		public static void CreateNewSimpleQuery() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("a.*")));
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("b")));
			query.From.Table(ObjectName.Parse("tab1"));


			var expected = new SqlStringBuilder();
			expected.AppendLine("SELECT a.*, b");
			expected.Append("  FROM tab1");

			Assert.False(query.From.IsEmpty);
			Assert.Equal(expected.ToString(), query.ToString());
		}

		[Fact]
		public static void MakeNewNaturalJoinQuery() {
			var query = new SqlQueryExpression();
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("a.*")));
			query.Items.Add(SqlExpression.Reference(ObjectName.Parse("b.*")));
			query.From.Table(ObjectName.Parse("table1"), "a");
			query.From.Table(ObjectName.Parse("table2"), "b");

			var expected = new SqlStringBuilder();
			expected.AppendLine("SELECT a.*, b.*");
			expected.Append("  FROM table1 AS a, table2 AS b");
			Assert.Equal(expected.ToString(), query.ToString());
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

			var expected = new SqlStringBuilder();
			expected.AppendLine("SELECT a.*, b.*");
			expected.Append("  FROM table1 AS a INNER JOIN table2 AS b ON a.id = b.a_id");

			Assert.Equal(expected.ToString(), query.ToString());
		}
	}
}