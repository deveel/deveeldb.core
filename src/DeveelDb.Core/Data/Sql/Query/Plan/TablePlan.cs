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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class TablePlan {
		public TablePlan(IQueryPlanNode plan, ObjectName[] columnNames, string[] uniqueNames) {
			Plan = plan;
			ColumnNames = columnNames;
			UniqueNames = uniqueNames;
		}

		public IQueryPlanNode Plan { get; set; }

		public ObjectName[] ColumnNames { get; }

		public string[] UniqueNames { get; }

		public bool IsUpdated { get; private set; } = false;

		public TablePlan LeftPlan { get; private set; }

		public JoinType LeftJoinType { get; private set; } = JoinType.None;

		public SqlExpression LeftOnExpression { get; private set; }

		public TablePlan RightPlan { get; private set; }

		public JoinType RightJoinType { get; private set; } = JoinType.None;

		public SqlExpression RightOnExpression { get; private set; }

		public void SetRightJoin(TablePlan plan, JoinType joinType, SqlExpression onExpression) {
			RightPlan = plan;
			RightJoinType = joinType;
			RightOnExpression = onExpression;
		}

		public void SetLeftJoin(TablePlan plan, JoinType joinType, SqlExpression onExpression) {
			LeftPlan = plan;
			LeftJoinType = joinType;
			LeftOnExpression = onExpression;
		}

		public void MergeBetween(TablePlan left, TablePlan right) {
			if (left.RightPlan != right) {
				if (left.RightPlan != null) {
					SetRightJoin(left.RightPlan, left.RightJoinType, left.RightOnExpression);
					RightPlan.LeftPlan = this;
				}
				if (right.LeftPlan != null) {
					SetLeftJoin(right.LeftPlan, right.LeftJoinType, right.LeftOnExpression);
					LeftPlan.RightPlan = this;
				}
			}
			if (left.LeftPlan != right) {
				if (LeftPlan == null && left.LeftPlan != null) {
					SetLeftJoin(left.LeftPlan, left.LeftJoinType, left.LeftOnExpression);
					LeftPlan.RightPlan = this;
				}
				if (RightPlan == null && right.RightPlan != null) {
					SetRightJoin(right.RightPlan, right.RightJoinType, right.RightOnExpression);
					RightPlan.LeftPlan = this;
				}
			}
		}

		public bool ContainsColumn(ObjectName columnName) {
			return ColumnNames.Any(x => x.Equals(columnName));
		}

		public bool ContainsUniqueName(string name) {
			return UniqueNames.Any(x => String.Equals(x, name, StringComparison.Ordinal));
		}

		public void MarkAsUpdated() {
			IsUpdated = true;
		}

		public void UpdatePlan(IQueryPlanNode plan) {
			Plan = plan;
			MarkAsUpdated();
		}

		public TablePlan Copy() {
			return new TablePlan(Plan, ColumnNames, UniqueNames);
		}
	}
}
