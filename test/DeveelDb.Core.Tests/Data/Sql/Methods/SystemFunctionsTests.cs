﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Methods {
	public class SystemFunctionsTests : IDisposable {
		private IContext context;

		public SystemFunctionsTests() {
			var scope = new ServiceContainer();
			scope.AddMethodRegistry<SystemFunctionProvider>();

			var groups = new Dictionary<ObjectName, IList<SqlObject>>();
			groups[new ObjectName("a")] = new List<SqlObject> {
				SqlObject.Integer(2),
				SqlObject.Integer(45)
			};

			var groupResolver = new Mock<IGroupResolver>();
			groupResolver.SetupGet(x => x.GroupId)
				.Returns(0);
			groupResolver.SetupGet(x => x.Size)
				.Returns(groups[new ObjectName("a")].Count);
			groupResolver.Setup(x => x.ResolveReferenceAsync(It.IsAny<ObjectName>(), It.IsAny<long>()))
				.Returns<ObjectName, long>((name, index) => {
					IList<SqlObject> group;
					groups.TryGetValue(name, out group);
					return Task.FromResult(group[(int) index]);
				});

			var refResolver = new Mock<IReferenceResolver>();
			refResolver.Setup(x => x.ResolveType(It.IsAny<ObjectName>()))
				.Returns(PrimitiveTypes.Integer());

			groupResolver.Setup(x => x.GetResolver(It.IsAny<long>()))
				.Returns(refResolver.Object);

			scope.RegisterInstance<IGroupResolver>(groupResolver.Object);
			scope.RegisterInstance<IReferenceResolver>(refResolver.Object);

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(scope);
			mock.SetupGet(x => x.ContextName)
				.Returns("test");

			context = mock.Object;
		}


		[Fact]
		public async void CountRef() {
			var function = SqlExpression.Function(new ObjectName("count"),
				new[] { new InvokeArgument(SqlExpression.Reference(new ObjectName("a"))) });

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;

			Assert.Equal(SqlObject.BigInt(2), value);
		}


		[Fact]
		public async void CountAll() {
			var function = SqlExpression.Function(new ObjectName("count"),
				new[] { new InvokeArgument(SqlExpression.Reference(new ObjectName("*"))) });

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;

			Assert.Equal(SqlObject.BigInt(2), value);
		}

		[Fact]
		public async void Sum() {
			var function = SqlExpression.Function(new ObjectName("sum"),
				new[] { new InvokeArgument(SqlExpression.Reference(new ObjectName("a"))) });

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;

			Assert.Equal(SqlObject.Integer(47), value);
		}

		[Fact]
		public async void Max() {
			var function = SqlExpression.Function(new ObjectName("max"),
				new[] { new InvokeArgument(SqlExpression.Reference(new ObjectName("a"))) });

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;

			Assert.Equal(SqlObject.Integer(45), value);
		}

		[Fact]
		public async void Min() {
			var function = SqlExpression.Function(new ObjectName("min"),
				new[] { new InvokeArgument(SqlExpression.Reference(new ObjectName("a"))) });

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;

			Assert.Equal(SqlObject.Integer(2), value);
		}

		[Fact]
		public async void Avg() {
			var function = SqlExpression.Function(new ObjectName("avg"),
				new[] { new InvokeArgument(SqlExpression.Reference(new ObjectName("a"))) });

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;

			Assert.Equal(SqlObject.Numeric((SqlNumber) 23.5), value);
		}


		[Fact]
		public async void StDev() {
			var function = SqlExpression.Function(new ObjectName("stdev"),
				new[] { new InvokeArgument(SqlExpression.Reference(new ObjectName("a"))) });

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;

			Assert.Equal(SqlObject.Numeric((SqlNumber)30.40559159102154), value);
		}

		[Fact]
		public async void First() {
			var function = SqlExpression.Function(new ObjectName("first"),
				new[] { new InvokeArgument(SqlExpression.Reference(new ObjectName("a"))) });

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;

			Assert.Equal(SqlObject.Integer(2), value);
		}

		[Fact]
		public async void Last() {
			var function = SqlExpression.Function(new ObjectName("last"),
				new[] { new InvokeArgument(SqlExpression.Reference(new ObjectName("a"))) });

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;

			Assert.Equal(SqlObject.Integer(45), value);
		}

		[Fact]
		public async void GroupId() {
			var function = SqlExpression.Function(new ObjectName("group_id"));

			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;
			Assert.Equal(SqlObject.Integer(0), value);
		}


		public void Dispose() {
			context.Dispose();
		}
	}
}