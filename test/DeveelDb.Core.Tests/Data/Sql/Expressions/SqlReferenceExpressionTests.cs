using System;

using Deveel.Data.Services;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public class SqlReferenceExpressionTests : IDisposable {
		private IContext context;

		public SqlReferenceExpressionTests() {
			var resolver = new Mock<IReferenceResolver>();
			resolver.Setup(x => x.ResolveReference(It.Is<ObjectName>(name => name.Name == "a")))
				.Returns<ObjectName>(name => SqlObject.String(new SqlString("test string to resolve")));

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());
			context = mock.Object;

			context.Scope.RegisterInstance<IReferenceResolver>(resolver.Object);
		}

		[Theory]
		[InlineData("a.*")]
		[InlineData("a.b.c")]
		public void CreateReference(string name) {
			var objName = ObjectName.Parse(name);
			var exp = SqlExpression.Reference(objName);

			Assert.NotNull(exp.ReferenceName);
			Assert.Equal(objName, exp.ReferenceName);
		}

		[Theory]
		[InlineData("a.*")]
		[InlineData("a.b.c")]
		public void GetReferenceString(string name) {
			var objName = ObjectName.Parse(name);
			var exp = SqlExpression.Reference(objName);

			var sql = exp.ToString();
			Assert.Equal(name, sql);
		}

		[Theory]
		[InlineData("a")]
		public void ReduceReference(string name) {
			var objName = ObjectName.Parse(name);

			var exp = SqlExpression.Reference(objName);
			var result = exp.Reduce(context);

			Assert.NotNull(result);
		}

		[Theory]
		[InlineData("b")]
		public void ReduceNotFoundReference(string name) {
			var objName = ObjectName.Parse(name);

			var exp = SqlExpression.Reference(objName);
			var result = exp.Reduce(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression) result).Value;

			Assert.Equal(SqlObject.Unknown, value);
		}

		[Fact]
		public void ReduceOutsideScope() {
			var objName = ObjectName.Parse("a.b");

			var exp = SqlExpression.Reference(objName);

			Assert.Throws<SqlExpressionException>(() => exp.Reduce(null));
		}

		public void Dispose() {
			if (context != null)
				context.Dispose();
		}
	}
}