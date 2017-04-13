using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlAggregateFunctionDelegate : SqlAggregateFunction {
		private readonly Func<IterateContext, Task> iterate;
		private Func<InitializeContext, Task> preparation;
		private Func<MergeContext, Task> aggregation;

		public SqlAggregateFunctionDelegate(SqlFunctionInfo functionInfo, Func<IterateContext, Task> iterate) 
			: base(functionInfo) {
			if (iterate == null)
				throw new ArgumentNullException(nameof(iterate));

			this.iterate = iterate;
		}

		public SqlAggregateFunctionDelegate(SqlFunctionInfo functionInfo, Action<IterateContext> iterate)
			: this(functionInfo, context => {
				iterate(context);
				return Task.CompletedTask;
			}) {
		}

		public void Seed(Func<InitializeContext, Task> prepare) {
			preparation = prepare;
		}

		public void Aggregate(Func<MergeContext, Task> aggregate) {
			aggregation = aggregate;
		}

		protected override Task IterateAsync(IterateContext context) {
			return iterate(context);
		}

		protected override Task MergeAsync(MergeContext context) {
			if (aggregation != null)
				return aggregation(context);

			return Task.CompletedTask;
		}

		protected override Task InitializeAsync(InitializeContext context) {
			if (preparation != null)
				return preparation(context);

			return Task.CompletedTask;
		}
	}
}