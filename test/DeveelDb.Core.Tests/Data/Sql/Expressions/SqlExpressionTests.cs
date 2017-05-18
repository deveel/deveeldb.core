using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;

using Xunit;

namespace Deveel.Data.Sql.Expressions {
	public static class SqlExpressionTests {
		[Theory]
		[InlineData(65775.499)]
		[InlineData("The quick brown fox")]
		public static void CreateConstant(object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			Assert.NotNull(exp.Value);
			Assert.Equal(obj, exp.Value);
		}

		[Theory]
		[InlineData(65775.499)]
		[InlineData("The quick brown fox")]
		public static void SerializeConstant(object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var result = BinarySerializeUtil.Serialize(exp);

			Assert.Equal(exp.ExpressionType, result.ExpressionType);
			Assert.Equal(exp.Value, result.Value);
		}

		[Theory]
		[InlineData(SqlExpressionType.Equal, 6577.494, 449.004)]
		[InlineData(SqlExpressionType.Add, 323, 12)]
		public static void CreateBinary(SqlExpressionType expressionType, object value1, object value2) {
			var obj1 = SqlObject.New(SqlValueUtil.FromObject(value1));
			var obj2 = SqlObject.New(SqlValueUtil.FromObject(value2));

			var exp = SqlExpression.Binary(expressionType, SqlExpression.Constant(obj1), SqlExpression.Constant(obj2));

			Assert.NotNull(exp);
			Assert.NotNull(exp.Left);
			Assert.NotNull(exp.Right);
		}

		[Theory]
		[InlineData(2201.112, 203, "2201.112 + 203")]
		public static void GetAddString(object value1, object value2, string expected) {
			AssertString(SqlExpression.Add, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 - 2293.1102")]
		public static void GetSubtractString(object value1, object value2, string expected) {
			AssertString(SqlExpression.Subtract, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 / 2293.1102")]
		public static void GetDivideString(object value1, object value2, string expected) {
			AssertString(SqlExpression.Divide, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 * 2293.1102")]
		public static void GetMultiplyString(object value1, object value2, string expected) {
			AssertString(SqlExpression.Multiply, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 % 2293.1102")]
		public static void GetModuloString(object value1, object value2, string expected) {
			AssertString(SqlExpression.Modulo, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 = 2293.1102")]
		public static void GetEqualString(object value1, object value2, string expected) {
			AssertString(SqlExpression.Equal, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 <> 2293.1102")]
		public static void GetNotEqualString(object value1, object value2, string expected) {
			AssertString(SqlExpression.NotEqual, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 > 2293.1102")]
		public static void GetGreaterThanString(object value1, object value2, string expected) {
			AssertString(SqlExpression.GreaterThan, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 < 2293.1102")]
		public static void GetLessThanString(object value1, object value2, string expected) {
			AssertString(SqlExpression.LessThan, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 >= 2293.1102")]
		public static void GetGreaterThanOrEqualString(object value1, object value2, string expected) {
			AssertString(SqlExpression.GreaterThanOrEqual, value1, value2, expected);
		}

		[Theory]
		[InlineData(12, 2293.1102, "12 <= 2293.1102")]
		public static void GetLessThanOrEqualString(object value1, object value2, string expected) {
			AssertString(SqlExpression.LessThanOrEqual, value1, value2, expected);
		}

		[Theory]
		[InlineData(true, false, "TRUE AND FALSE")]
		public static void GetAndString(object value1, object value2, string expected) {
			AssertString(SqlExpression.And, value1, value2, expected);
		}

		[Theory]
		[InlineData(true, false, "TRUE OR FALSE")]
		public static void GetOrString(object value1, object value2, string expected) {
			AssertString(SqlExpression.Or, value1, value2, expected);
		}

		[Theory]
		[InlineData(SqlExpressionType.Or, true, false)]
		[InlineData(SqlExpressionType.Add, "the quick", "brown fox")]
		public static void SerializeBinary(SqlExpressionType type, object value1, object value2) {
			var binary = SqlExpression.Binary(type, SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value1))),
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value2))));

			var result = BinarySerializeUtil.Serialize(binary);

			Assert.Equal(binary.ExpressionType, result.ExpressionType);
			Assert.Equal(binary.Left.ExpressionType, result.Left.ExpressionType);
			Assert.Equal(binary.Right.ExpressionType, result.Right.ExpressionType);
		}

		private static SqlBinaryExpression BinaryExpression(Func<SqlExpression, SqlExpression, SqlBinaryExpression> factory,
			object value1, object value2) {
			var left = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value1)));
			var right = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value2)));

			return factory(left, right);
		}

		private static void AssertString(Func<SqlExpression, SqlExpression, SqlBinaryExpression> factory,
			object value1, object value2, string expected) {
			var exp = BinaryExpression(factory, value1, value2);
			var sql = exp.ToString();
			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData(SqlExpressionType.Like, "antonello", "anto%")]
		[InlineData(SqlExpressionType.NotLike, "the quick brown fox", "the brown%")]
		public static void CreateStringMatch(SqlExpressionType expressionType, string value, string pattern) {
			var exp = SqlExpression.StringMatch(expressionType,
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value))),
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(pattern))), null);

			Assert.NotNull(exp);
			Assert.Equal(expressionType, exp.ExpressionType);
			Assert.NotNull(exp.Left);
			Assert.NotNull(exp.Pattern);
		}

		[Theory]
		[InlineData(SqlExpressionType.Like, "antonello", "anto%")]
		[InlineData(SqlExpressionType.NotLike, "the quick brown fox", "the brown%")]
		public static void SerializeStringMatch(SqlExpressionType expressionType, string value, string pattern) {
			var exp = SqlExpression.StringMatch(expressionType,
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value))),
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(pattern))), null);

			var result = BinarySerializeUtil.Serialize(exp);

			Assert.Equal(exp.ExpressionType, result.ExpressionType);
		}

		[Theory]
		[InlineData(SqlExpressionType.Like, "antonello", "anto%", true)]
		[InlineData(SqlExpressionType.Like, "antonello", "apto%", false)]
		[InlineData(SqlExpressionType.NotLike, "the quick brown fox", "the brown%", true)]
		[InlineData(SqlExpressionType.NotLike, "the quick brown fox", "the quick%", false)]
		public static async Task ReduceStringMatch(SqlExpressionType expressionType, string value, string pattern, bool expected) {
			var exp = SqlExpression.StringMatch(expressionType,
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value))),
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(pattern))), null);

			Assert.NotNull(exp);
			Assert.Equal(expressionType, exp.ExpressionType);
			Assert.NotNull(exp.Left);
			Assert.NotNull(exp.Pattern);

			var reduced = await exp.ReduceAsync(null);

			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);

			var result = ((SqlConstantExpression) reduced).Value;
			Assert.IsType<SqlBoolean>(result.Value);
			Assert.Equal(expected, (bool) ((SqlBoolean)result.Value));
		}

		[Theory]
		[InlineData(SqlExpressionType.Like, "antonello", "anto%", "\\", true)]
		[InlineData(SqlExpressionType.Like, "antonello", "apto%", "|", false)]
		[InlineData(SqlExpressionType.NotLike, "the quick brown fox", "the brown%", "\\", true)]
		[InlineData(SqlExpressionType.NotLike, "the quick brown fox", "the quick%", "\\", false)]
		public static async Task ReduceStringMatchWithEscape(SqlExpressionType expressionType, string value, string pattern, string escape, bool expected) {
			var exp = SqlExpression.StringMatch(expressionType,
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value))),
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(pattern))),
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(escape))));

			Assert.NotNull(exp);
			Assert.Equal(expressionType, exp.ExpressionType);
			Assert.NotNull(exp.Left);
			Assert.NotNull(exp.Pattern);

			var reduced = await exp.ReduceAsync(null);

			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);

			var result = ((SqlConstantExpression)reduced).Value;
			Assert.IsType<SqlBoolean>(result.Value);
			Assert.Equal(expected, (bool)((SqlBoolean)result.Value));
		}


		[Theory]
		[InlineData(SqlExpressionType.Like, "antonello", "anto%", "'antonello' LIKE 'anto%'")]
		[InlineData(SqlExpressionType.NotLike, "the quick brown fox", "the quick%", "'the quick brown fox' NOT LIKE 'the quick%'")]
		public static void GetStringMatchString(SqlExpressionType expressionType, string value, string pattern, string expected) {
			var exp = SqlExpression.StringMatch(expressionType,
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value))),
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(pattern))), null);

			var sql = exp.ToString();
			Assert.Equal(expected, sql);
		}


		[Theory]
		[InlineData(SqlExpressionType.Like, "antonello", "anto%", "\\", "'antonello' LIKE 'anto%' ESCAPE '\\'")]
		public static void GetStringMatchStringWithEscape(SqlExpressionType expressionType, string value, string pattern, string escape, string expected) {
			var exp = SqlExpression.StringMatch(expressionType,
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(value))),
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(pattern))),
				SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(escape))));

			var sql = exp.ToString();
			Assert.Equal(expected, sql);
		}


		[Theory]
		[InlineData(345)]
		public static async Task ReduceConstant(object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			Assert.False(exp.CanReduce);
			Assert.NotNull(exp.Value);

			var reduced = await exp.ReduceAsync(null);

			Assert.IsType<SqlConstantExpression>(reduced);
			Assert.Equal(obj, ((SqlConstantExpression) reduced).Value);
		}

		[Theory]
		[InlineData(SqlExpressionType.Equal, 34, 34, true)]
		[InlineData(SqlExpressionType.NotEqual, 190, 21, true)]
		[InlineData(SqlExpressionType.GreaterThan, 12.02e32, 23.98, true)]
		[InlineData(SqlExpressionType.GreaterThanOrEqual, 110, 20, true)]
		[InlineData(SqlExpressionType.LessThan, 67, 98, true)]
		[InlineData(SqlExpressionType.LessThanOrEqual, "abc1234", "abc12345", false)]
		[InlineData(SqlExpressionType.Add, 45, 45, 90)]
		[InlineData(SqlExpressionType.Subtract, 102, 30, 72)]
		[InlineData(SqlExpressionType.Multiply, 22, 2, 44)]
		[InlineData(SqlExpressionType.Divide, 100, 2, 50)]
		[InlineData(SqlExpressionType.Is, true, true, true)]
		[InlineData(SqlExpressionType.IsNot, 22.09, false, true)]
		[InlineData(SqlExpressionType.Or, true, false, true)]
		[InlineData(SqlExpressionType.XOr, 113, 56, 73)]
		[InlineData(SqlExpressionType.And, true, false, false)]
		public static async Task ReduceBinary(SqlExpressionType expressionType, object value1, object value2, object expected) {
			var obj1 = SqlObject.New(SqlValueUtil.FromObject(value1));
			var obj2 = SqlObject.New(SqlValueUtil.FromObject(value2));

			var exp = SqlExpression.Binary(expressionType, SqlExpression.Constant(obj1), SqlExpression.Constant(obj2));

			Assert.True(exp.CanReduce);

			var reduced = await exp.ReduceAsync(null);

			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);

			var result = ((SqlConstantExpression) reduced).Value;
			var expectedResult = SqlObject.New(SqlValueUtil.FromObject(expected));

			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData(SqlExpressionType.Equal, 34, 34, "34 = 34")]
		[InlineData(SqlExpressionType.NotEqual, 190, 21, "190 <> 21")]
		[InlineData(SqlExpressionType.GreaterThan, 12.02, 23.98, "12.02 > 23.98")]
		[InlineData(SqlExpressionType.GreaterThanOrEqual, 110, 20, "110 >= 20")]
		[InlineData(SqlExpressionType.LessThan, 67, 98, "67 < 98")]
		[InlineData(SqlExpressionType.LessThanOrEqual, "abc1234", "abc12345", "'abc1234' <= 'abc12345'")]
		[InlineData(SqlExpressionType.Add, 45, 45, "45 + 45")]
		[InlineData(SqlExpressionType.Subtract, 102, 30, "102 - 30")]
		[InlineData(SqlExpressionType.Multiply, 22, 2, "22 * 2")]
		[InlineData(SqlExpressionType.Divide, 100, 2, "100 / 2")]
		[InlineData(SqlExpressionType.Is, true, true, "TRUE IS TRUE")]
		[InlineData(SqlExpressionType.IsNot, 22.09, false, "22.09 IS NOT FALSE")]
		[InlineData(SqlExpressionType.Or, true, false, "TRUE OR FALSE")]
		[InlineData(SqlExpressionType.XOr, 113, 56, "113 XOR 56")]
		[InlineData(SqlExpressionType.And, true, false, "TRUE AND FALSE")]
		public static void GetBinaryString(SqlExpressionType expressionType, object value1, object value2, string expected) {
			var obj1 = SqlObject.New(SqlValueUtil.FromObject(value1));
			var obj2 = SqlObject.New(SqlValueUtil.FromObject(value2));

			var exp = SqlExpression.Binary(expressionType, SqlExpression.Constant(obj1), SqlExpression.Constant(obj2));

			var s = exp.ToString();
			Assert.Equal(expected, s);
		}


		[Theory]
		[InlineData(SqlExpressionType.Add, 445, 9032.11)]
		public static void GetBinarySqlType(SqlExpressionType expressionType, object value1, object value2) {
			var obj1 = SqlObject.New(SqlValueUtil.FromObject(value1));
			var obj2 = SqlObject.New(SqlValueUtil.FromObject(value2));

			var exp = SqlExpression.Binary(expressionType, SqlExpression.Constant(obj1), SqlExpression.Constant(obj2));

			var sqltType = exp.Type;
			var wider = obj1.Type.Wider(obj2.Type);

			Assert.Equal(wider, sqltType);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34)]
		[InlineData(SqlExpressionType.Negate, 455.43)]
		[InlineData(SqlExpressionType.Not, true)]
		public static void CreateUnary(SqlExpressionType expressionType, object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			Assert.NotNull(exp.Operand);
			Assert.IsType<SqlConstantExpression>(exp.Operand);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34)]
		[InlineData(SqlExpressionType.Negate, 455.43)]
		[InlineData(SqlExpressionType.Not, true)]
		public static void SerializeUnary(SqlExpressionType expressionType, object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			var result = BinarySerializeUtil.Serialize(exp);

			Assert.Equal(exp.ExpressionType, result.ExpressionType);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34, 22.34)]
		[InlineData(SqlExpressionType.Negate, 455.43, -455.43)]
		[InlineData(SqlExpressionType.Not, true, false)]
		public static async Task ReduceUnary(SqlExpressionType expressionType, object value, object expected) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			Assert.NotNull(exp.Operand);
			Assert.IsType<SqlConstantExpression>(exp.Operand);

			Assert.True(exp.CanReduce);

			var reduced = await exp.ReduceAsync(null);

			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);

			var expectedResult = SqlObject.New(SqlValueUtil.FromObject(expected));
			Assert.Equal(expectedResult, ((SqlConstantExpression) reduced).Value);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34, "+22.34")]
		[InlineData(SqlExpressionType.Negate, 455.43, "-455.43")]
		[InlineData(SqlExpressionType.Not, true, "~TRUE")]
		public static void GetUnaryString(SqlExpressionType expressionType, object value, string expected) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			var s = exp.ToString();
			Assert.Equal(expected, s);
		}

		[Theory]
		[InlineData(SqlExpressionType.UnaryPlus, 22.34)]
		[InlineData(SqlExpressionType.Negate, 455.43)]
		[InlineData(SqlExpressionType.Not, true)]
		public static void GetUnaryType(SqlExpressionType expressionType, object value) {
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var operand = SqlExpression.Constant(obj);
			var exp = SqlExpression.Unary(expressionType, operand);

			var sqlType = exp.Type;
			Assert.Equal(obj.Type, sqlType);
		}

		[Theory]
		[InlineData(5634.99, SqlTypeCode.Double, -1, -1)]
		public static void NewCast(object value, SqlTypeCode destTypeCode, int p, int s) {
			var targetType = PrimitiveTypes.Type(destTypeCode, new {precision = p, scale = s, maxSize = p, size = p});
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var cast = SqlExpression.Cast(exp, targetType);

			Assert.NotNull(cast.Value);
			Assert.NotNull(cast.TargetType);
		}

		[Theory]
		[InlineData(5634.99, SqlTypeCode.Double, -1, -1)]
		public static void SerializeCast(object value, SqlTypeCode destTypeCode, int p, int s) {
			var targetType = PrimitiveTypes.Type(destTypeCode, new { precision = p, scale = s, maxSize = p, size = p });
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var cast = SqlExpression.Cast(exp, targetType);
			var result = BinarySerializeUtil.Serialize(cast);

			Assert.IsType<SqlConstantExpression>(result.Value);
		}

		[Theory]
		[InlineData(5634.99, SqlTypeCode.Double, -1, -1, (double) 5634.99)]
		[InlineData("true", SqlTypeCode.Boolean, -1, -1, true)]
		public static async Task ReduceCast(object value, SqlTypeCode destTypeCode, int p, int s, object expected) {
			var targetType = PrimitiveTypes.Type(destTypeCode, new {precision = p, scale = s, maxSize = p, size = p});
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var cast = SqlExpression.Cast(exp, targetType);

			Assert.True(cast.CanReduce);

			var reduced = await cast.ReduceAsync(null);
			Assert.NotNull(reduced);
			Assert.IsType<SqlConstantExpression>(reduced);

			var result = ((SqlConstantExpression) reduced).Value;

			Assert.NotNull(result);
			Assert.Equal(destTypeCode, result.Type.TypeCode);

			var expectedResult = SqlObject.New(SqlValueUtil.FromObject(expected));
			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData(5634.99, SqlTypeCode.Double, -1, -1)]
		[InlineData("TRUE", SqlTypeCode.Boolean, -1, -1)]
		public static void GetCastType(object value, SqlTypeCode destTypeCode, int p, int s) {
			var targetType = PrimitiveTypes.Type(destTypeCode, new { precision = p, scale = s, maxSize = p, size = p });
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var cast = SqlExpression.Cast(exp, targetType);

			Assert.Equal(targetType, cast.TargetType);
		}

		[Theory]
		[InlineData(5634.99, SqlTypeCode.Double, -1, -1, "CAST(5634.99 AS DOUBLE)")]
		[InlineData("TRUE", SqlTypeCode.Boolean, -1, -1, "CAST('TRUE' AS BOOLEAN)")]
		public static void GetCastString(object value, SqlTypeCode destTypeCode, int p, int s, string expected) {
			var targetType = PrimitiveTypes.Type(destTypeCode, new {precision = p, scale = s, maxSize = p, size = p});
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var cast = SqlExpression.Cast(exp, targetType);
			var sql = cast.ToString();

			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData("a.b", true)]
		public static void CreateReferenceAssign(string name, object value) {
			var objName = ObjectName.Parse(name);
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var refAssign = SqlExpression.ReferenceAssign(objName, exp);

			Assert.NotNull(refAssign.ReferenceName);
			Assert.NotNull(refAssign.Value);

			Assert.Equal(objName, refAssign.ReferenceName);
		}

		[Theory]
		[InlineData("a.b", true)]
		public static void SerializeReferenceAssign(string name, object value) {
			var objName = ObjectName.Parse(name);
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var refAssign = SqlExpression.ReferenceAssign(objName, exp);
			var result = BinarySerializeUtil.Serialize(refAssign);

			Assert.Equal(objName, result.ReferenceName);
			Assert.IsType<SqlConstantExpression>(result.Value);
		}


		[Theory]
		[InlineData("a.b", true, "a.b = TRUE")]
		public static void GetReferenceAssignString(string name, object value, string expected) {
			var objName = ObjectName.Parse(name);
			var obj = SqlObject.New(SqlValueUtil.FromObject(value));
			var exp = SqlExpression.Constant(obj);

			var refAssign = SqlExpression.ReferenceAssign(objName, exp);
			var sql = refAssign.ToString();

			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData(true, 223.21, 11, "CASE WHEN TRUE THEN 223.21 ELSE 11 END")]
		public static void GetConditionString(bool test, object ifTrue, object ifFalse, string expected) {
			var testExp = SqlExpression.Constant(SqlObject.Boolean(test));
			var ifTrueExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifTrue)));
			var ifFalseExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifFalse)));

			var condition = SqlExpression.Condition(testExp, ifTrueExp, ifFalseExp);

			var sql = condition.ToString();
			Assert.Equal(expected, sql);
		}

		[Theory]
		[InlineData(true, 223.21, 11)]
		public static void SerializeCondition(bool test, object ifTrue, object ifFalse) {
			var testExp = SqlExpression.Constant(SqlObject.Boolean(test));
			var ifTrueExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifTrue)));
			var ifFalseExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifFalse)));

			var condition = SqlExpression.Condition(testExp, ifTrueExp, ifFalseExp);
			var result = BinarySerializeUtil.Serialize(condition);

			Assert.NotNull(result.Test);
			Assert.NotNull(result.IfTrue);
			Assert.NotNull(result.IfFalse);
		}


		[Theory]
		[InlineData(true, "I am", "You are", "I am")]
		[InlineData(false, "I am", "You are", "You are")]
		public static async Task  ReduceCondition(bool test, object ifTrue, object ifFalse, object expected) {
			var testExp = SqlExpression.Constant(SqlObject.Boolean(test));
			var ifTrueExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifTrue)));
			var ifFalseExp = SqlExpression.Constant(SqlObject.New(SqlValueUtil.FromObject(ifFalse)));

			var condition = SqlExpression.Condition(testExp, ifTrueExp, ifFalseExp);

			var result = await condition.ReduceAsync(null);

			Assert.IsType<SqlConstantExpression>(result);

			var expectedValue = SqlObject.New(SqlValueUtil.FromObject(expected));
			Assert.Equal(expectedValue, ((SqlConstantExpression)result).Value);
		}
	}
}