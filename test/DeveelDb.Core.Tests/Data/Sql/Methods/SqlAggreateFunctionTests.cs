using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Methods {
	public class SqlAggreateFunctionTests : IDisposable {
		private IContext context;

		public SqlAggreateFunctionTests() {
			var group = new List<SqlObject> {
				SqlObject.Integer(33),
				SqlObject.Integer(22),
				SqlObject.Integer(1)

			};

			var refResolver = new Mock<IReferenceResolver>();
			refResolver.Setup(x => x.ResolveType(It.IsAny<ObjectName>()))
				.Returns<ObjectName>(name => PrimitiveTypes.Integer());
			refResolver.Setup(x => x.ResolveReferenceAsync(It.IsAny<ObjectName>()))
				.Returns<ObjectName>(name => Task.FromResult(group[0]));

			var resolverMock = new Mock<IGroupResolver>();
			resolverMock.SetupGet(x => x.Size)
				.Returns(group.Count);
			resolverMock.Setup(x => x.ResolveReferenceAsync(It.IsAny<ObjectName>(), It.IsAny<long>()))
				.Returns<ObjectName, long>((name, index) => Task.FromResult(group[(int)index]));
			resolverMock.Setup(x => x.GetResolver(It.IsAny<long>()))
				.Returns(refResolver.Object);

			var container = new ServiceContainer();
			container.RegisterInstance<IGroupResolver>(resolverMock.Object);

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(container);

			context = mock.Object;
		}

		[Fact]
		public async Task ExecuteWithReference() {
			var info = new SqlFunctionInfo(new ObjectName("count"), PrimitiveTypes.BigInt());
			info.Parameters.Add(new SqlParameterInfo("a", PrimitiveTypes.VarChar()));

			var function = new SqlAggregateFunctionDelegate(info, accumulate => {
				SqlObject r;
				if (accumulate.IsFirst) {
					r = accumulate.Current;
				} else {
					var x = accumulate.Accumulation;
					var y = accumulate.Current;

					r = x.Add(y);
				}

				accumulate.SetResult(r);
			});

			var result = await function.ExecuteAsync(context, SqlExpression.Reference(ObjectName.Parse("a")));

			Assert.NotNull(result);
			Assert.NotNull(result.ReturnedValue);
			Assert.True(result.HasReturnedValue);

			Assert.IsType<SqlConstantExpression>(result.ReturnedValue);

			var value = ((SqlConstantExpression) result.ReturnedValue).Value;

			Assert.Equal(SqlObject.Integer(56), value);
		}

		[Fact]
		public async Task ExecuteWithValues() {
			var info = new SqlFunctionInfo(new ObjectName("count"), PrimitiveTypes.BigInt());
			info.Parameters.Add(new SqlParameterInfo("a", PrimitiveTypes.VarChar()));

			var function = new SqlAggregateFunctionDelegate(info, accumulate => {
				SqlObject r;
				if (accumulate.IsFirst) {
					r = accumulate.Current;
				} else {
					var x = accumulate.Accumulation;
					var y = accumulate.Current;

					r = x.Add(y);
				}

				accumulate.SetResult(r);
			});

			var arg = SqlExpression.Multiply(SqlExpression.Reference(new ObjectName("a")),
				SqlExpression.Constant(SqlObject.Integer(2)));

			var result = await function.ExecuteAsync(context, arg);

			Assert.NotNull(result);
			Assert.NotNull(result.ReturnedValue);
			Assert.True(result.HasReturnedValue);

			Assert.IsType<SqlConstantExpression>(result.ReturnedValue);

			var value = ((SqlConstantExpression)result.ReturnedValue).Value;

			Assert.Equal(SqlObject.Integer(198), value);
		}

		public void Dispose() {
			context.Dispose();
		}
	}
}