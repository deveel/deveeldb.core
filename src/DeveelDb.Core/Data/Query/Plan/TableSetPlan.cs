using System;
using System.Collections.Generic;
using System.Linq;

namespace Deveel.Data.Query.Plan {
	class TableSetPlan {
		private List<TablePlan> tables;
		private bool hasJoins;

		public TableSetPlan() {
			tables = new List<TablePlan>();
			hasJoins = false;
		}

		public void AddTablePlan(TablePlan plan) {
			tables.Add(plan);
			hasJoins = true;
		}

		public TablePlan FindTablePlan(ObjectName columnName) {
			if (tables.Count == 1)
				return tables[0];

			var plan = tables.FirstOrDefault(x => x.ContainsColumn(columnName));
			if (plan == null)
				throw new InvalidOperationException($"Unable to find any table that references {columnName}");

			return plan;
		}

		public int IndexOfPlan(TablePlan plan) {
			return tables.FindIndex(x => x.Equals(plan));
		}
	}
}