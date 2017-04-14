using System;

using Deveel.Data.Services;
using Deveel.Data.Sql.Variables;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public class SqlVariableExpressionTests : IDisposable {
		private IContext context;

		public SqlVariableExpressionTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());
			context = mock.Object;

			var value = SqlExpression.Constant(SqlObject.New(new SqlBoolean(false)));
			var variable = new Variable("a", PrimitiveTypes.Boolean(), false, value);

			var resolver = new Mock<IVariableResolver>();
			resolver.Setup(x => x.ResolveVariable(It.Is<string>(s => s == "a"), It.IsAny<bool>()))
				.Returns<string, bool>((name, ignoreCase) => variable);

			context.RegisterService<IVariableResolver, VariableManager>();
			context.RegisterService<VariableManager>();
			var manager = context.ResolveService<VariableManager>();
			manager.CreateVariable(new VariableInfo("b", PrimitiveTypes.VarChar(150), false, null));

			context.Scope.RegisterInstance<IVariableResolver>(resolver.Object);
			context.Scope.RegisterInstance(manager);
		}

		[Theory]
		[InlineData("a")]
		public void CreateVarRef(string name) {
			var varRef = SqlExpression.Variable(name);

			Assert.NotEmpty(varRef.VariableName);
			Assert.Equal(name, varRef.VariableName);
		}

		[Theory]
		[InlineData("a", true)]
		public void CreateVarAssign(string name, object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var varRef = SqlExpression.VariableAssign(name, exp);
			Assert.NotNull(varRef.Value);
			Assert.Equal(exp, varRef.Value);
			Assert.Equal(name, varRef.VariableName);
		}

		[Theory]
		[InlineData("a", ":a")]
		[InlineData("ab_test", ":ab_test")]
		public void GetVariableString(string name, string expected) {
			var varRef = SqlExpression.Variable(name);
			var sql = varRef.ToString();

			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("a", true, ":a := TRUE")]
		public void GetVarAssingString(string name, object value, string expected) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var varRef = SqlExpression.VariableAssign(name, exp);
			var sql = varRef.ToString();

			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("a")]
		public void ReduceVariable(string name) {
			var varRef = SqlExpression.Variable(name);

			Assert.True(varRef.CanReduce);
			var result = varRef.Reduce(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;
			Assert.NotEqual(SqlObject.Unknown, value);
		}

		[Theory]
		[InlineData("b")]
		public void ReduceNotFoundVariable(string name) {
			var varRef = SqlExpression.Variable(name);

			Assert.True(varRef.CanReduce);
			var result = varRef.Reduce(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression) result).Value;
			Assert.Equal(SqlObject.Unknown, value);
		}

		[Theory]
		[InlineData("a", true)]
		public void ReduceVarAssign(string name, object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var varRef = SqlExpression.VariableAssign(name, exp);

			Assert.True(varRef.CanReduce);

			var result = varRef.Reduce(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);
		}

		[Theory]
		[InlineData("b", "test", SqlTypeCode.VarChar, 50)]
		public void GetSqlTypeOfVarAssign(string name, object value, SqlTypeCode typeCode, int p) {
			var exp = SqlExpression.VariableAssign(name, SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value))));

			var varType = exp.GetSqlType(context);
			var type = PrimitiveTypes.Type(typeCode, new {precision = p, maxSize = p, size = p});

			Assert.Equal(type, varType);
		}

		[Theory]
		[InlineData("a", SqlTypeCode.Boolean, 50)]
		public void GetSqlType(string name, SqlTypeCode typeCode, int p) {
			var exp = SqlExpression.Variable(name);

			var varType = exp.GetSqlType(context);
			var type = PrimitiveTypes.Type(typeCode, new { precision = p, maxSize = p, size = p });

			Assert.Equal(type, varType);
		}

		public void Dispose() {
			if (context != null)
				context.Dispose();
		}
	}
}