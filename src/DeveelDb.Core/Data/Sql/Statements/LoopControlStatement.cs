// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


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

		internal LoopControlStatement(SerializationInfo info)
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

		protected override void AppendTo(SqlStringBuilder builder) {
			if (ControlType == LoopControlType.Continue) {
				builder.Append("CONTINUE");
			} else {
				builder.Append("EXIT");
			}

			if (!String.IsNullOrWhiteSpace(Label)) {
				builder.AppendFormat(" '{0}'", Label);
			}

			if (When != null) {
				builder.Append(" WHEN ");
				When.AppendTo(builder);
			}

			builder.Append(";");
		}
	}
}