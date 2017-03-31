using System;
using System.Linq;

using Xunit;

namespace Deveel.Data.Sql {
	public class SqlQueryTests {
		[Fact]
		public void NewSimpleQuery() {
			var query = new SqlQuery("SELECT * FROM b");

			Assert.Equal("SELECT * FROM b", query.Text);
			Assert.NotNull(query.Parameters);
			Assert.Empty(query.Parameters);
			Assert.Equal(QueryParameterStyle.Default, query.ParameterStyle);
		}

		[Fact]
		public void NewQueryWithNamedParameter() {
			var query = new SqlQuery("INSERT INTO a (col1) VALUES (:v1)", QueryParameterStyle.Named);
			query.Parameters.Add(new QueryParameter("v1", PrimitiveTypes.Boolean(), SqlBoolean.True));

			Assert.Equal("INSERT INTO a (col1) VALUES (:v1)", query.Text);
			Assert.Equal(QueryParameterStyle.Named, query.ParameterStyle);
			Assert.NotNull(query.Parameters);
			Assert.NotEmpty(query.Parameters);
			Assert.Equal(1, query.Parameters.Count);

			var param = query.Parameters.ElementAt(0);
			Assert.NotNull(param);
			Assert.Equal("v1", param.Name);
			Assert.NotNull(param.SqlType);
			Assert.IsType<SqlBooleanType>(param.SqlType);
			Assert.Equal(QueryParameterDirection.In, param.Direction);
			Assert.IsType<SqlBoolean>(param.Value);
			Assert.Equal(SqlBoolean.True, param.Value);
		}
	}
}