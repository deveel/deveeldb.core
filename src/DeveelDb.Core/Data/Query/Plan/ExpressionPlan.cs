using System;

namespace Deveel.Data.Query.Plan {
	abstract class ExpressionPlan : IComparable<ExpressionPlan> {
		protected ExpressionPlan(float optimizeFactor) {
			OptimizeFactor = optimizeFactor;
		}

		public float OptimizeFactor { get; }

		public int CompareTo(ExpressionPlan other) {
			if (ReferenceEquals(this, other))
				return 0;
			if (ReferenceEquals(null, other))
				return 1;

			return OptimizeFactor.CompareTo(other.OptimizeFactor);
		}

		public abstract void AddToPlan(TableSetPlan plan);
	}
}