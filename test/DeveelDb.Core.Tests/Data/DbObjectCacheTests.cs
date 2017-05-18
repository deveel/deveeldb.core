using System;
using System.Collections.Generic;
using System.Text;

using Deveel.Data.Sql;
using Deveel.Data.Sql.Variables;

using Xunit;

namespace Deveel.Data {
	public class DbObjectCacheTests {
		[Fact]
		public static void SetAndGetObject() {
			var cache = new DbObjectCache<Variable>();

			cache.SetObject(new Variable("a", PrimitiveTypes.Integer()));

			Variable variable;
			Assert.True(cache.TryGetObject(new ObjectName("a"), out variable));
			Assert.NotNull(variable);
		}

		[Theory]
		[InlineData("a", "A", true, true)]
		[InlineData("ab", "AB", true, true)]
		[InlineData("ab", "aB", false, false)]
		[InlineData("a", "b", true, false)]
		public static void ResolveName(string name, string toResolve, bool ignoreCase, bool expected) {
			var cache = new DbObjectCache<Variable>();

			cache.SetObject(new Variable(name, PrimitiveTypes.Integer()));

			ObjectName resolved;
			Assert.Equal(expected, cache.TryResolveName(ObjectName.Parse(toResolve), ignoreCase, out resolved));
			Assert.Equal(expected, resolved != null);
		}
	}
}