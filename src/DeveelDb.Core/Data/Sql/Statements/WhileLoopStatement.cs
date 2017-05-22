using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public sealed class WhileLoopStatement : LoopStatement {
		public WhileLoopStatement(SqlExpression condition)
			: this(condition, null) {
		}

		public WhileLoopStatement(SqlExpression condition, string label)
			: base(label) {
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));

			Condition = condition;
		}

		private WhileLoopStatement(SerializationInfo info)
			: base(info) {
			Condition = info.GetValue<SqlExpression>("condition");
		}

		public SqlExpression Condition { get; }

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("condition", Condition);

			base.GetObjectData(info);
		}

		protected override async Task<bool> CanLoopAsync(StatementContext context) {
			var result = await Condition.ReduceToConstantAsync(context);
			if (result.IsNull || result.IsUnknown)
				throw new SqlExpressionException("The condition expression reduced to an invalid state");

			return result.IsTrue;
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			if (!String.IsNullOrWhiteSpace(Label)) {
				builder.AppendFormat("<<{0}>>", Label);
				builder.AppendLine();
			}

			builder.Append("WHILE ");
			Condition.AppendTo(builder);
			builder.AppendLine();

			builder.AppendLine("LOOP");
			builder.Indent();

			foreach (var statement in Statements) {
				statement.AppendTo(builder);
				builder.AppendLine();
			}

			builder.DeIndent();
			builder.Append("END LOOP;");
		}
	}
}