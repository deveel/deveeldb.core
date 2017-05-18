using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public class LoopControlStatement : SqlStatement, IPlSqlStatement {
		public LoopControlStatement(LoopControlType controlType)
			: this(controlType, (SqlExpression) null) {
		}

		public LoopControlStatement(LoopControlType controlType, SqlExpression when)
			: this(controlType, null, when) {
		}

		public LoopControlStatement(LoopControlType controlType, string label) 
			: this(controlType, label, null) {
		}

		public LoopControlStatement(LoopControlType controlType, string label, SqlExpression when) {
			ControlType = controlType;
			Label = label;
			When = when;
		}

		private LoopControlStatement(SerializationInfo info)
			: base(info) {
			ControlType = info.GetValue<LoopControlType>("type");
			Label = info.GetString("label");
			When = info.GetValue<SqlExpression>("when");
		}

		public LoopControlType ControlType { get; }

		public string Label { get; }

		public SqlExpression When { get; }

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("type", ControlType);
			info.SetValue("label", Label);
		}

		protected override SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			var when = When;
			if (when != null)
				when = when.Prepare(preparer);

			return new LoopControlStatement(ControlType, Label, when);
		}

		protected override async Task ExecuteStatementAsync(StatementContext context) {
			if (When != null) {
				var when = await When.ReduceToConstantAsync(context);
				if (when.IsFalse)
					return;
			}

			context.ControlLoop(ControlType, Label);
		}
	}
}