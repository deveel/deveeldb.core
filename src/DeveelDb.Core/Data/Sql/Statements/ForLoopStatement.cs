using System;
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Statements {
	public sealed class ForLoopStatement : LoopStatement {
		public ForLoopStatement(string indexName, SqlExpression lowerBound, SqlExpression upperBound)
			: this(indexName, lowerBound, upperBound, false) {
		}

		public ForLoopStatement(string indexName, SqlExpression lowerBound, SqlExpression upperBound, bool reverse) {
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

		protected override StatementContext CreateContext(IContext parent, string name) {
			return new StatementContext(parent, name, this, scope => scope.Register<VariableManager>());
		}

		protected override SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			var lower = LowerBound.Prepare(preparer);
			var upper = UpperBound.Prepare(preparer);

			var loop = new ForLoopStatement(IndexName, lower, upper, Reverse);
			foreach (var statement in Statements) {
				loop.Statements.Add(statement);
			}

			return loop;
		}

		internal override LoopStatement CreateNew() {
			return new ForLoopStatement(IndexName, LowerBound, UpperBound, Reverse);
		}

		protected override async Task InitializeAsync(StatementContext context) {
			var lowerBound = await LowerBound.ReduceToConstantAsync(context);
			var upperBound = await UpperBound.ReduceToConstantAsync(context);

			context.Metadata["lowerBound"] = lowerBound;
			context.Metadata["upperBound"] = upperBound;

			var variableManager = (context as IContext).Scope.Resolve<VariableManager>();
			if (Reverse) {
				variableManager.AssignVariable(IndexName, SqlExpression.Constant(upperBound), context);
			} else {
				variableManager.AssignVariable(IndexName, SqlExpression.Constant(lowerBound), context);
			}

			await base.InitializeAsync(context);
		}

		protected override async Task<bool> CanLoopAsync(StatementContext context) {
			var variableManager = (context as IContext).Scope.Resolve<VariableManager>();
			var variable = variableManager.GetVariable(IndexName);

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
			var variableManager = (context as IContext).Scope.Resolve<VariableManager>();
			var variable = variableManager.GetVariable(IndexName);

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
	}
}