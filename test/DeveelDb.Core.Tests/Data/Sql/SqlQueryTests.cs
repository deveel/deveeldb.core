using System;
using System.Linq;
using System.Reflection;

using Xunit;

namespace Deveel.Data.Sql {
	public static class SqlQueryTests {
		[Fact]
		public static void NewSimpleQuery() {
			var query = new SqlQuery("SELECT * FROM b");

			Assert.Equal("SELECT * FROM b", query.Text);
			Assert.NotNull(query.Parameters);
			Assert.Empty(query.Parameters);
			Assert.Equal(SqlQueryParameterNaming.Default, query.ParameterNaming);
		}

		[Fact]
		public static void NewQueryWithNamedParameter() {
			var query = new SqlQuery("INSERT INTO a (col1) VALUES (:v1)", SqlQueryParameterNaming.Named);
			query.Parameters.Add(new SqlQueryParameter("v1", PrimitiveTypes.Boolean(), SqlBoolean.True));

			Assert.Equal("INSERT INTO a (col1) VALUES (:v1)", query.Text);
			Assert.Equal(SqlQueryParameterNaming.Named, query.ParameterNaming);
			Assert.NotNull(query.Parameters);
			Assert.NotEmpty(query.Parameters);
			Assert.Equal(1, query.Parameters.Count);

			var param = query.Parameters.ElementAt(0);
			Assert.NotNull(param);
			Assert.Equal("v1", param.Name);
			Assert.NotNull(param.SqlType);
			Assert.IsType<SqlBooleanType>(param.SqlType);
			Assert.Equal(SqlParameterDirection.In, param.Direction);
			Assert.IsType<SqlBoolean>(param.Value);
			Assert.Equal(SqlBoolean.True, param.Value);
		}

		[Theory]
		[InlineData(SqlQueryParameterNaming.Marker, ":var1")]
		[InlineData(SqlQueryParameterNaming.Named, "?")]
		public static void AddInvalidStyledQueryParameter(SqlQueryParameterNaming naming, string paramName) {
			var query = new SqlQuery($"INSERT INTO a (col1) VALUES ({paramName})", naming);
			Assert.Throws<ArgumentException>(() => query.Parameters.Add(new SqlQueryParameter(paramName, PrimitiveTypes.Boolean(), SqlBoolean.True)));
		}

		[Theory]
		[InlineData("a")]
		[InlineData(":a")]
		public static void AddDoubleNamedQueryParameter(string paramName) {
			var query = new SqlQuery($"INSERT INTO a (col1) VALUES ({paramName})", SqlQueryParameterNaming.Named);
			query.Parameters.Add(new SqlQueryParameter(paramName, PrimitiveTypes.Boolean(), SqlBoolean.True));
			Assert.Throws<ArgumentException>(
				() => query.Parameters.Add(new SqlQueryParameter(paramName, PrimitiveTypes.Boolean(), SqlBoolean.True)));
		}

		[Theory]
		[InlineData(SqlQueryParameterNaming.Marker, "?", "?")]
		[InlineData(SqlQueryParameterNaming.Named, ":var1", ":var2")]
		public static void ChangeStyle(SqlQueryParameterNaming newNaming, string paramName1, string paramName2) {
			var query = new SqlQuery($"SELECT * FROM a WHERE a.col1 = {paramName1} AND a.col2 = {paramName2}");
			query.Parameters.Add(new SqlQueryParameter(paramName1, PrimitiveTypes.String(),new SqlString($"val of {paramName1}")));
			query.Parameters.Add(new SqlQueryParameter(paramName2, PrimitiveTypes.Boolean(), SqlBoolean.False));

			Assert.Equal(SqlQueryParameterNaming.Default, query.ParameterNaming);
			ChangeNamingOf(query, newNaming);
			Assert.Equal(newNaming, query.ParameterNaming);
		}

		[Theory]
		[InlineData(SqlQueryParameterNaming.Marker, "?", "var1")]
		[InlineData(SqlQueryParameterNaming.Named, ":var1", "?")]
		public static void ChangeStyleWithInvalidParameters(SqlQueryParameterNaming newNaming, string paramName1, string paramName2) {
			var query = new SqlQuery($"SELECT * FROM a WHERE a.col1 = {paramName1} AND a.col2 = {paramName2}");
			query.Parameters.Add(new SqlQueryParameter(paramName1, PrimitiveTypes.String(), new SqlString($"val of {paramName1}")));
			query.Parameters.Add(new SqlQueryParameter(paramName2, PrimitiveTypes.Boolean(), SqlBoolean.False));

			Assert.Equal(SqlQueryParameterNaming.Default, query.ParameterNaming);

			Assert.Throws<ArgumentException>(() => ChangeNamingOf(query, newNaming));
		}

		[Theory]
		[InlineData(SqlQueryParameterNaming.Marker)]
		[InlineData(SqlQueryParameterNaming.Named)]
		public static void ChangeStyleOfNonDefault(SqlQueryParameterNaming naming) {
			var query = new SqlQuery("EXECUTE sp_insertData", naming);
			Assert.Throws<InvalidOperationException>(() => ChangeNamingOf(query, naming));
		}

		private static void ChangeNamingOf(SqlQuery query, SqlQueryParameterNaming newNaming) {
			try {
				typeof(SqlQuery).GetMethod("ChangeNaming", BindingFlags.Instance | BindingFlags.NonPublic)
					.Invoke(query, new object[] {newNaming});
			} catch(TargetInvocationException ex) {
				throw ex.InnerException;
			}
		}
	}
}