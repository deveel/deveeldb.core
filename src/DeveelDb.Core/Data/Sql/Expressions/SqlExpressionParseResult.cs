using System;

namespace Deveel.Data.Sql.Expressions {
    public struct SqlExpressionParseResult {
        private SqlExpressionParseResult(SqlExpression expression, bool valid, string[] errors) {
            Expression = expression;
            Valid = valid;
            Errors = errors;
        }

        public SqlExpression Expression { get; }

        public bool Valid { get; }

        public string[] Errors { get; }

        public static SqlExpressionParseResult Success(SqlExpression expression)
            => new SqlExpressionParseResult(expression, true, new string[0]);

        public static SqlExpressionParseResult Fail(params string[] errors)
            => new SqlExpressionParseResult(null, false, errors);
    }
}