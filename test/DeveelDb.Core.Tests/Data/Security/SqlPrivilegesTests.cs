using System;

using Xunit;

namespace Deveel.Data.Security {
	public class SqlPrivilegesTests {
		[Theory]
		[InlineData("INSERT")]
		[InlineData("Insert")]
		[InlineData("SELECT")]
		[InlineData("Update")]
		[InlineData("usage")]
		[InlineData("delete")]
		[InlineData("References")]
		[InlineData("List")]
		[InlineData("Execute")]
		[InlineData("Drop")]
		[InlineData("Create")]
		public static void ParsePrivilege(string s) {
			var privilege = SqlPrivileges.Resolver.ResolvePrivilege(s);
			Assert.NotEqual(Privilege.None, privilege);
		}

		[Fact]
		public static void FormatToString() {
			Assert.Equal("INSERT", SqlPrivileges.Insert.ToString());
			Assert.Equal("DELETE", SqlPrivileges.Delete.ToString());
			Assert.Equal("UPDATE", SqlPrivileges.Update.ToString());
			Assert.Equal("USAGE", SqlPrivileges.Usage.ToString());
			Assert.Equal("EXECUTE", SqlPrivileges.Execute.ToString());
			Assert.Equal("LIST", SqlPrivileges.List.ToString());
			Assert.Equal("REFERENCES", SqlPrivileges.References.ToString());
			Assert.Equal("SELECT, INSERT", (SqlPrivileges.Select + SqlPrivileges.Insert).ToString());
			Assert.Equal("DROP, ALTER, CREATE", (SqlPrivileges.Create + SqlPrivileges.Alter + SqlPrivileges.Drop).ToString());
		}

		[Fact]
		public static void RemoveFromPrivilege() {
			var privileges = SqlPrivileges.Insert + SqlPrivileges.Delete;
			var result = privileges - SqlPrivileges.Insert;

			Assert.Equal(SqlPrivileges.Delete, result);
		}

		[Fact]
		public static void RemoveNotDefinedPrivilege() {
			var privileges = SqlPrivileges.Insert + SqlPrivileges.Delete;
			var result = privileges - SqlPrivileges.Update;

			Assert.Equal(privileges, result);
		}
	}
}