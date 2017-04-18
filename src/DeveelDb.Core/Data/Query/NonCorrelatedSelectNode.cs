using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Query {
	public sealed class NonCorrelatedSelectNode : BranchQueryPlanNode {
		public NonCorrelatedSelectNode(IQueryPlanNode left, IQueryPlanNode right, ObjectName[] leftColumns,
			SqlExpressionType op, SqlExpressionType subOp)
			: base(left, right) {
			LeftColumnNames = leftColumns;
			Operator = op;
			SubOperator = subOp;
		}

		public ObjectName[] LeftColumnNames { get; }

		public SqlExpressionType Operator { get; }

		public SqlExpressionType SubOperator { get; }

		protected override void GetData(IDictionary<string, object> data) {
			for (int i = 0; i < LeftColumnNames.Length; i++) {
				data[$"column[{i}]"] = LeftColumnNames[i];
			}

			data["operator"] = Operator.ToString().ToUpperInvariant();
			data["sub-operator"] = SubOperator.ToString().ToUpperInvariant();
		}

		public override async Task<ITable> ReduceAsync(IContext context) {
			var leftResult = await Left.ReduceAsync(context);
			var rightResult = await Right.ReduceAsync(context);

			return await leftResult.SelectNonCorrelatedAsync(LeftColumnNames, Operator, SubOperator, rightResult);
		}
	}
}