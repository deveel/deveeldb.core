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

		public SqlAggregateFunctionDelegate(SqlFunctionInfo functionInfo, Func<IterateContext, SqlObject> iterate)
			: this(functionInfo, context => {
				var result = iterate(context);
				context.SetResult(result);
			}) {
		}

		public void Initialize(Func<InitializeContext, Task> prepare) {
			preparation = prepare;
		}

		public void Merge(Func<MergeContext, Task> aggregate) {
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