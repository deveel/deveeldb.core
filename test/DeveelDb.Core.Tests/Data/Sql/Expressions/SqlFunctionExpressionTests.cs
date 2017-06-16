using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Methods;

using Moq;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public class SqlFunctionExpressionTests : IDisposable {
		private IContext context;

		public SqlFunctionExpressionTests() {
			var mock = new Mock<IContext>();
			mock.SetupGet(x => x.Scope)
				.Returns(new ServiceContainer());
			context = mock.Object;

			var methodInfo = new SqlFunctionInfo(ObjectName.Parse("sys.func1"), PrimitiveTypes.Integer());
			methodInfo.Parameters.Add(new SqlParameterInfo("a", PrimitiveTypes.VarChar(155)));
			methodInfo.Parameters.Add(new SqlParameterInfo("b", PrimitiveTypes.Integer(),
				SqlExpression.Constant(SqlObject.Null)));
			var method = new SqlFunctionDelegate(methodInfo, ctx => {
				return Task.FromResult(ctx.Value("a").Add(ctx.Value("b")));
			});

			var resolver = new Mock<IMethodResolver>();
			resolver.Setup(x => x.ResolveMethod(It.IsAny<IContext>(), It.IsAny<Invoke>()))
				.Returns<IContext, Invoke>((context, invoke) => method);

			context.Scope.RegisterInstance<IMethodResolver>(resolver.Object);
		}

		[Fact]
		public void CreateNew() {
			var exp = SqlExpression.Function(ObjectName.Parse("sys.func2"),
				new[] {new InvokeArgument(SqlExpression.Constant(SqlObject.Bit(false)))});

			Assert.Equal(SqlExpressionType.Function, exp.ExpressionType);
			Assert.NotNull(exp.Arguments);
			Assert.NotEmpty(exp.Arguments);
			Assert.Equal(1, exp.Arguments.Length);
		}

		[Fact]
		public void Serialize() {
			var exp = SqlExpression.Function(ObjectName.Parse("sys.func2"),
				new[] { new InvokeArgument(SqlExpression.Constant(SqlObject.Bit(false))) });

			var result = BinarySerializeUtil.Serialize(exp);

			Assert.Equal(exp.FunctionName, result.FunctionName);
			Assert.Equal(exp.Arguments.Length, result.Arguments.Length);
		}

		[Fact]
		public void GetSqlType() {
			var function = SqlExpression.Function(ObjectName.Parse("sys.Func1"), 
				new InvokeArgument("a", SqlObject.String(new SqlString("foo"))));
		
			Assert.True(function.IsReference);
			var type = function.GetSqlType(context);

			Assert.Equal(PrimitiveTypes.Integer(), type);
		}

		[Fact]
		public async Task ReduceFromExisting() {
			var function = SqlExpression.Function(ObjectName.Parse("sys.Func1"), 
				new InvokeArgument("a", SqlObject.String(new SqlString("foo"))));
			Assert.True(function.CanReduce);
			var result = await function.ReduceAsync(context);

			Assert.NotNull(result);
			Assert.IsType<SqlConstantExpression>(result);

			var value = ((SqlConstantExpression) result).Value;
			Assert.Equal(SqlObject.Null, value);
		}

		[Fact]
		public static void GetNamedString() {
			var function = SqlExpression.Function(ObjectName.Parse("sys.Func1"), new InvokeArgument("a", SqlObject.BigInt(33)));

			const string sql = "sys.Func1(a => 33)";
			Assert.Equal(sql, function.ToString());
		}

		[Fact]
		public static void GetAnonString() {
			var function = SqlExpression.Function(ObjectName.Parse("sys.Func1"), new InvokeArgument(SqlObject.BigInt(33)));

			const string sql = "sys.Func1(33)";
			Assert.Equal(sql, function.ToString());
		}

		[Theory]
		[InlineData("sys.Func1(a => 56)", "sys.Func1", "a", "INT", 56)]
		[InlineData("fun2(a => 'test')", "fun2", "a", "STRING", "test")]
		public static void ParseStringWithNamedParameter(string s, string funcName, string paramName, string paramType, object paramValue) {
			var exp = SqlExpression.Parse(s);

			Assert.NotNull(exp);
			Assert.Equal(SqlExpressionType.Function, exp.ExpressionType);
			Assert.IsType<SqlFunctionExpression>(exp);

			var function = (SqlFunctionExpression) exp;

			Assert.Equal(ObjectName.Parse(funcName), function.FunctionName);
			Assert.NotEmpty(function.Arguments);
			Assert.Equal(1, function.Arguments.Length);

			var param = function.Arguments[0];

			Assert.NotNull(param);
			Assert.True(param.IsNamed);
			Assert.Equal(paramName, param.ParameterName);
			Assert.IsType<SqlConstantExpression>(param.Value);

			var type = SqlType.Parse(paramType);
			var value = new SqlObject(type, SqlValueUtil.FromObject(paramValue));

			Assert.Equal(value, ((SqlConstantExpression) param.Value).Value);
		}

		[Theory]
		[InlineData("sys.Func1(56, 'test1')", "sys.Func1", "INT", 56, "STRING", "test1")]
		public static void ParseStringWithAnonParameter(string s, string funcName, string param1Type, object paramValue1, string param2Type, object paramValue2) {
			var exp = SqlExpression.Parse(s);

			Assert.NotNull(exp);
			Assert.Equal(SqlExpressionType.Function, exp.ExpressionType);
			Assert.IsType<SqlFunctionExpression>(exp);

			var function = (SqlFunctionExpression)exp;

			Assert.Equal(ObjectName.Parse(funcName), function.FunctionName);
			Assert.NotEmpty(function.Arguments);
			Assert.Equal(2, function.Arguments.Length);

			var param1 = function.Arguments[0];
			var param2 = function.Arguments[1];

			Assert.NotNull(param1);
			Assert.NotNull(param2);

			Assert.False(param1.IsNamed);
			Assert.False(param2.IsNamed);

			Assert.IsType<SqlConstantExpression>(param1.Value);
			Assert.IsType<SqlConstantExpression>(param2.Value);

			var pType1 = SqlType.Parse(param1Type);
			var pType2 = SqlType.Parse(param2Type);

			var value1 = new SqlObject(pType1, SqlValueUtil.FromObject(paramValue1));
			var value2 = new SqlObject(pType2, SqlValueUtil.FromObject(paramValue2));

			Assert.Equal(value1, ((SqlConstantExpression)param1.Value).Value);
			Assert.Equal(value2, ((SqlConstantExpression)param2.Value).Value);
		}

		[Theory]
		[InlineData("TRIM (LEADING ' ' FROM '  test')")]
		[InlineData("TRIM(BOTH ' ' FROM ' test ')")]
		[InlineData("TRIM(' test')")]
		[InlineData("TRIM(TRAILING ' ' FROM 'test ')")]
		public static void ParseSqlTrimFunction(string s) {
			var exp = SqlExpression.Parse(s);

			Assert.NotNull(exp);
			Assert.IsType<SqlFunctionExpression>(exp);

			var func = (SqlFunctionExpression) exp;
			Assert.Equal("SQL_TRIM", func.FunctionName.FullName);
			Assert.NotEmpty(func.Arguments);
			Assert.Equal(3, func.Arguments.Length);
		}

		[Theory]
		[InlineData("EXTRACT(DAY FROM '1980-06-04')")]
		[InlineData("EXTRACT(YEAR FROM birth_date)")]
		public static void ParseSqlExtractFunction(string s) {
			var exp = SqlExpression.Parse(s);

			Assert.NotNull(exp);
			Assert.IsType<SqlFunctionExpression>(exp);

			var func = (SqlFunctionExpression)exp;
			Assert.Equal("SQL_EXTRACT", func.FunctionName.FullName);
			Assert.NotEmpty(func.Arguments);
			Assert.Equal(2, func.Arguments.Length);
		}

		[Theory]
		[InlineData("CURRENT_TIME", "TIME")]
		[InlineData("CURRENT_TIMESTAMP", "TIMESTAMP")]
		[InlineData("CURRENT_DATE", "DATE")]
		public static void ParseCurrentTimeFunction(string s, string functionName) {
			var exp = SqlExpression.Parse(s);

			Assert.NotNull(exp);
			Assert.IsType<SqlFunctionExpression>(exp);

			var func = (SqlFunctionExpression)exp;
			Assert.Equal(functionName, func.FunctionName.FullName);
		}

		[Theory]
		[InlineData("TIMESTAMP '1980-04-06'")]
		public static void ParseTimeStampFunction(string s) {
			var exp = SqlExpression.Parse(s);

			Assert.NotNull(exp);
			Assert.IsType<SqlFunctionExpression>(exp);

			var func = (SqlFunctionExpression)exp;
			Assert.Equal("TOTIMESTAMP", func.FunctionName.FullName);
			Assert.NotEmpty(func.Arguments);
			Assert.Equal(1, func.Arguments.Length);
		}

		public void Dispose() {
			context?.Dispose();
		}
	}
}