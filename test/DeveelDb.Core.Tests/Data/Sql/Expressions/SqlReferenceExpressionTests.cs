using System;
using System.Threading.Tasks;

using Deveel.Data.Services;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public class SqlReferenceExpressionTests : IDisposable {
		private IContext context;

		public SqlReferenceExpressionTests() {
			var resolver = new Mock<IReferenceResolver>();
			resolver.Setup(x => x.ResolveReferenceAsync(It.Is<ObjectName>(name => name.Name == "a")))
				.Returns<ObjectName>(name => Task.FromResult(SqlObject.String(new SqlString("test string to resolve"))));
			resolver.Setup(x => x.ResolveType(It.IsAny<ObjectName>()))
				.Returns(PrimitiveTypes.String());

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
		public async Task ReduceReference(string name) {
			var objName = ObjectName.Parse(name);

			var exp = SqlExpression.Reference(objName);
			var result = await exp.ReduceAsync(context);

			Assert.NotNull(result);
		}

		[Theory]
		[InlineData("b")]
		public async Task ReduceNotFoundReference(string name) {
			var objName = ObjectName.Parse(name);

			var exp = SqlExpression.Reference(objName);
			var result = await exp.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression) result).Value;

			Assert.Equal(SqlObject.Unknown, value);
		}

		[Fact]
		public async Task ReduceOutsideScope() {
			var objName = ObjectName.Parse("a.b");

			var exp = SqlExpression.Reference(objName);

			await Assert.ThrowsAsync<SqlExpressionException>(() => exp.ReduceAsync(null));
		}

		[Fact]
		public void GetSqlType() {
			var name = ObjectName.Parse("a.b");
			var exp = SqlExpression.Reference(name);

			var type = exp.GetSqlType(context);
			Assert.Equal(PrimitiveTypes.String(), type);
		}

		public void Dispose() {
			if (context != null)
				context.Dispose();
		}
	}
}