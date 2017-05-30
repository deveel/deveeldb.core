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
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Statements {
	public sealed class ForLoopStatement : LoopStatement {
		public ForLoopStatement(string indexName, SqlExpression lowerBound, SqlExpression upperBound)
			: this(indexName, lowerBound, upperBound, null) {
		}

		public ForLoopStatement(string indexName, SqlExpression lowerBound, SqlExpression upperBound, string label)
			: this(indexName, lowerBound, upperBound, false, label) {
		}

		public ForLoopStatement(string indexName, SqlExpression lowerBound, SqlExpression upperBound, bool reverse)
			: this(indexName, lowerBound, upperBound, reverse, null) {
		}

		public ForLoopStatement(string indexName, SqlExpression lowerBound, SqlExpression upperBound, bool reverse, string label)
			: base(label) {
			if (String.IsNullOrEmpty(indexName))
				throw new ArgumentNullException(nameof(indexName));
			if (lowerBound == null)
				throw new ArgumentNullException(nameof(lowerBound));
			if (upperBound == null)
				throw new ArgumentNullException(nameof(upperBound));

			IndexName = indexName;
			LowerBound = lowerBound;
			UpperBound = upperBound;
			Reverse = reverse;
		}

		private ForLoopStatement(SerializationInfo info)
			: base(info) {
			IndexName = info.GetString("index");
			LowerBound = (SqlExpression) info.GetValue("lowerBound", typeof(SqlExpression));
			UpperBound = (SqlExpression) info.GetValue("upperBound", typeof(SqlExpression));
			Reverse = info.GetBoolean("reverse");
		}

		public string IndexName { get; private set; }

		public SqlExpression LowerBound { get; }

		public SqlExpression UpperBound { get; }

		public bool Reverse { get; }


		protected override SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			var lower = LowerBound.Prepare(preparer);
			var upper = UpperBound.Prepare(preparer);

			var loop = new ForLoopStatement(IndexName, lower, upper, Reverse);
			foreach (var statement in Statements) {
				loop.Statements.Add(statement);
			}

			return loop;
		}

		protected override SqlStatement PrepareStatement(IContext context) {
			var statement = new ForLoopStatement(IndexName, LowerBound, UpperBound, Reverse);
			foreach (var child in Statements) {
				var prepared = child.Prepare(context);
				statement.Statements.Add(prepared);
			}

			return statement;
		}

		internal override LoopStatement CreateNew() {
			return new ForLoopStatement(IndexName, LowerBound, UpperBound, Reverse);
		}

		protected override async Task InitializeAsync(StatementContext context) {
			var lowerBound = await LowerBound.ReduceToConstantAsync(context);
			var upperBound = await UpperBound.ReduceToConstantAsync(context);

			context.Metadata["lowerBound"] = lowerBound;
			context.Metadata["upperBound"] = upperBound;

			if (Reverse) {
				context.AssignVariable(IndexName, SqlExpression.Constant(upperBound));
			} else {
				context.AssignVariable(IndexName, SqlExpression.Constant(lowerBound));
			}

			await base.InitializeAsync(context);
		}

		protected override async Task<bool> CanLoopAsync(StatementContext context) {
			var variable = context.ResolveVariable(IndexName);

			var valueExp = await variable.Evaluate(context);
			 var value = await valueExp.ReduceToConstantAsync(context);

			if (Reverse) {
				var lowerBound = (SqlObject) context.Metadata["lowerBound"];
				if (value.LessOrEqualThan(lowerBound).IsTrue)
					return false;
			} else {
				var upperBound = (SqlObject) context.Metadata["upperBound"];
				if (value.GreaterThanOrEqual(upperBound).IsTrue)
					return false;
			}

			return await base.CanLoopAsync(context);
		}

		protected override async Task AfterLoopAsync(StatementContext context) {
			var variable = context.ResolveVariable(IndexName);

			var value = await variable.Evaluate(context);
			if (Reverse) {
				variable.SetValue(SqlExpression.Subtract(value, SqlExpression.Constant(SqlObject.BigInt(1))), context);
			} else {
				variable.SetValue(SqlExpression.Add(value, SqlExpression.Constant(SqlObject.BigInt(1))), context);
			}

			// TODO: resolve the variable from the context and increment
			await base.AfterLoopAsync(context);
		}

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("index", IndexName);
			info.SetValue("lowerBound", LowerBound);
			info.SetValue("upperBound", UpperBound);
			info.SetValue("reverse", Reverse);

			base.GetObjectData(info);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			if (!String.IsNullOrWhiteSpace(Label)) {
				builder.AppendFormat("<<{0}>>", Label);
				builder.AppendLine();
			}

			builder.AppendFormat("FOR {0} IN ", IndexName);
			LowerBound.AppendTo(builder);
			builder.Append("..");
			UpperBound.AppendTo(builder);
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