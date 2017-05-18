using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public sealed class ReturnStatement : SqlStatement, IPlSqlStatement {
		public ReturnStatement() 
			: this((SqlExpression) null) {
		}

		public ReturnStatement(SqlExpression value) {
			Value = value;
		}

		private ReturnStatement(SerializationInfo info)
			: base(info) {
			Value = info.GetValue<SqlExpression>("value");
		}

		public SqlExpression Value { get; }

		protected override SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			var value = Value;
			if (value != null)
				value = value.Prepare(preparer);

			return new ReturnStatement(value);
		}

		protected override Task ExecuteStatementAsync(StatementContext context) {
			context.Return(Value);
			return Task.CompletedTask;
		}

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("value", Value);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("RETURN");

			if (Value != null) {
				builder.Append(" ");
				Value.AppendTo(builder);
			}

			builder.Append(";");
		}
	}
}