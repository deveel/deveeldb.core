using System;
using System.Threading.Tasks;

using Deveel.Data.Services;
using Deveel.Data.Sql.Variables;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public class SqlVariableExpressionTests : IDisposable {
		private IContext context;

		public SqlVariableExpressionTests() {
			var scope = new ServiceContainer();

			var manager = new VariableManager();
			manager.CreateVariable(new VariableInfo("a", PrimitiveTypes.Boolean(), false, SqlExpression.Constant(SqlObject.Boolean(false))));
			manager.CreateVariable(new VariableInfo("b", PrimitiveTypes.VarChar(150), false, null));

			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(scope);
			mock.As<IVariableScope>()
				.SetupGet(x => x.Variables)
				.Returns(manager);

			context = mock.Object;
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
		public async Task ReduceVariable(string name) {
			var varRef = SqlExpression.Variable(name);

			Assert.True(varRef.CanReduce);
			var result = await varRef.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression)result).Value;
			Assert.NotEqual(SqlObject.Unknown, value);
		}

		[Theory]
		[InlineData("c")]
		public async Task ReduceNotFoundVariable(string name) {
			var varRef = SqlExpression.Variable(name);

			Assert.True(varRef.CanReduce);
			var result = await varRef.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression) result).Value;
			Assert.Equal(SqlObject.Unknown, value);
		}

		[Theory]
		[InlineData("a", true)]
		public async Task ReduceVarAssign(string name, object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var varRef = SqlExpression.VariableAssign(name, exp);

			Assert.True(varRef.CanReduce);

			var result = await varRef.ReduceAsync(context);

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

		[Theory]
		[InlineData(":a", "a")]
		[InlineData(":1", "1")]
		public static void ParseString(string s, string varName) {
			var exp = SqlExpression.Parse(s);

			Assert.NotNull(exp);
			Assert.IsType<SqlVariableExpression>(exp);

			var varExp = (SqlVariableExpression) exp;
			Assert.NotNull(varExp.VariableName);
			Assert.Equal(varName, varExp.VariableName);
		}

		public void Dispose() {
			if (context != null)
				context.Dispose();
		}
	}
}