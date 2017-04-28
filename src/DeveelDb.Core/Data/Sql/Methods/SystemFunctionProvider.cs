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
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class SystemFunctionProvider : SqlMethodRegistry {
		#region Utilities

		private void Register(string name, SqlParameterInfo[] parameters, SqlType returnType, Func<MethodContext, Task> body) {
			var functionInfo = new SqlFunctionInfo(ObjectName.Parse(name), returnType);
			foreach (var parameter in parameters) {
				functionInfo.Parameters.Add(parameter);
			}

			var function = new SqlFunctionDelegate(functionInfo, body);
			Register(function);
		}

		private void Register(string name, SqlParameterInfo param1, SqlParameterInfo param2, SqlParameterInfo param3, SqlType returnType,
			Func<MethodContext, Task> body) {
			Register(name, new []{param1, param2, param3}, returnType, body);
		}

		private void Register(string name, SqlParameterInfo param1, SqlParameterInfo param2, SqlType returnType,
			Func<MethodContext, Task> body) {
			Register(name, new[] { param1, param2}, returnType, body);
		}

		private void Register(string name, SqlParameterInfo param, SqlType returnType,
			Func<MethodContext, Task> body) {
			Register(name, new[] { param }, returnType, body);
		}

		private SqlParameterInfo Param(string name, SqlType type,
			SqlParameterDirection direction = SqlParameterDirection.In, SqlExpression defaultValue = null) {
			return new SqlParameterInfo(name, type, defaultValue, direction);
		}

		private SqlParameterInfo Deterministic(string name) {
			return Param(name, new SqlDeterministicType());
		}

		#endregion

		protected override void Initialize() {
			RegisterAggregates();
		}

		#region Aggregates

		private void RegisterAggregate(string name,
			SqlType returnType,
			Action<IterateContext> iterate,
			Func<InitializeContext, Task> initialize = null,
			Func<MergeContext, Task> merge = null) {
			RegisterAggregate(name, null, returnType, iterate, initialize, merge);
		}

		private void RegisterAggregate(string name,
			SqlParameterInfo param1,
			SqlType returnType,
			Action<IterateContext> iterate,
			Func<InitializeContext, Task> initialize = null,
			Func<MergeContext, Task> merge = null) {
			RegisterAggregate(name, param1, null, returnType, iterate, initialize, merge);
		}

		private void RegisterAggregate(string name,
			SqlParameterInfo param1, SqlParameterInfo param2,
			SqlType returnType,
			Action<IterateContext> iterate,
			Func<InitializeContext, Task> initialize = null,
			Func<MergeContext, Task> merge = null) {
			var methodInfo = new SqlFunctionInfo(new ObjectName(name), returnType);
			if (param1 != null)
				methodInfo.Parameters.Add(param1);
			if (param2 != null)
				methodInfo.Parameters.Add(param2);

			var aggregate = new SqlAggregateFunctionDelegate(methodInfo, iterate);
			aggregate.Initialize(initialize);
			aggregate.Merge(merge);

			Register(aggregate);
		}


		private void RegisterAggregates() {
			// COUNT(*)
			RegisterAggregate("COUNT", Deterministic("column"), PrimitiveTypes.VarNumeric(),
				iterate => {
					if (iterate.IsFirst) {
						iterate.SetResult(SqlObject.BigInt(1));
					} else {
						iterate.SetResult(iterate.Accumulation.Add(SqlObject.BigInt(1)));
					}
				},
				initialize => {
					var groupResolver = initialize.ResolveService<IGroupResolver>();
					var groupSize = groupResolver.Size;

					var argRef = (initialize.Input as SqlReferenceExpression)?.ReferenceName;
					if (groupSize == 0 || (argRef != null && argRef.IsGlob)) {
						initialize.SetResult(SqlExpression.Constant(SqlObject.BigInt(groupSize)), false);
					}

					return Task.CompletedTask;
				});

			// MIN
			RegisterAggregate("MIN", Deterministic("column"), PrimitiveTypes.VarNumeric(),
				iterate => {
					if (iterate.IsFirst) {
						iterate.SetResult(iterate.Current);
					} else if (iterate.Current.LessThan(iterate.Accumulation).IsTrue) {
						iterate.SetResult(iterate.Current);
					}
				});

			// MAX
			RegisterAggregate("MAX", Deterministic("column"), PrimitiveTypes.VarNumeric(),
				iterate => {
					if (iterate.IsFirst) {
						iterate.SetResult(iterate.Current);
					} else if (iterate.Current.GreaterThan(iterate.Accumulation).IsTrue) {
						iterate.SetResult(iterate.Current);
					} else if (iterate.Accumulation.LessThan(iterate.Current).IsTrue) {
						iterate.SetResult(iterate.Accumulation);
					}
				});

			// AVG
			RegisterAggregate("AVG", Deterministic("column"), PrimitiveTypes.VarNumeric(),
				iterate => {
					if (iterate.IsFirst) {
						iterate.SetResult(iterate.Current);
					} else {
						iterate.SetResult(iterate.Current.Add(iterate.Accumulation));
					}
				}, merge: merge => {
					var groupResolver = merge.ResolveService<IGroupResolver>();
					var groupSize = groupResolver.Size;

					var final = merge.Accumulated.Divide(SqlObject.BigInt(groupSize));
					merge.SetOutput(final);
					return Task.CompletedTask;
				});

			// STDEV
			RegisterAggregate("STDEV", Deterministic("column"), PrimitiveTypes.VarNumeric(), iterate => {
				var aggregator = iterate.ResolveService<AvgAggregator>();
				aggregator.Values.Add(iterate.Current);

				if (iterate.IsFirst) {
					iterate.SetResult(iterate.Current);
				} else {
					iterate.SetResult(iterate.Current.Add(iterate.Accumulation));
				}
			}, initialize => {
				var groupResolver = initialize.ResolveService<IGroupResolver>();
				var aggregator = new AvgAggregator {
					Values = new BigList<SqlObject>(groupResolver.Size)
				};

				initialize.MethodContext.RegisterInstance<AvgAggregator>(aggregator);
				return Task.CompletedTask;
			}, merge => {
				var groupResolver = merge.ResolveService<IGroupResolver>();
				var groupSize = groupResolver.Size;
				var aggregator = merge.ResolveService<AvgAggregator>();

				var avg = merge.Accumulated.Divide(SqlObject.BigInt(groupSize));
				var sums = aggregator.Values.Select(x => SqlMath.Pow((SqlNumber) x.Subtract(avg).Value, (SqlNumber) 2));
				var sum = SqlNumber.Zero;
				foreach (var number in sums) {
					sum += number;
				}

				var ret = SqlMath.Sqrt(sum / (SqlNumber)(groupSize - 1));
				merge.SetOutput(SqlObject.Numeric(ret));
				return Task.CompletedTask;
			});

			// SUM
			RegisterAggregate("SUM", Deterministic("column"), PrimitiveTypes.VarNumeric(),
				iterate => {
					if (iterate.IsFirst) {
						iterate.SetResult(iterate.Current);
					} else {
						iterate.SetResult(iterate.Current.Add(iterate.Accumulation));
					}
				});

			// LAST
			RegisterAggregate("LAST", Deterministic("column"), new SqlDeterministicType(),
				iterate => {
					var groupResolver = iterate.ResolveService<IGroupResolver>();
					var groupSize = groupResolver.Size;

					if (iterate.Offset == groupSize - 1) {
						iterate.SetResult(iterate.Current);
					} else {
						iterate.SetResult(SqlObject.Null);
					}
				});

			// FIRST
			RegisterAggregate("FIRST", Deterministic("column"), new SqlDeterministicType(),
				iterate => {
					if (iterate.IsFirst)
						iterate.SetResult(iterate.Current, false);
				});

			RegisterAggregate("GROUP_ID", PrimitiveTypes.Integer(),
				iterate => {
					// no-op
				}, merge: merge => {
					var groupResolver = merge.ResolveService<IGroupResolver>();
					merge.SetOutput(SqlObject.Integer(groupResolver.GroupId));
					return Task.CompletedTask;
				});
		}

		#region AvgAggregator

		class AvgAggregator {
			public BigList<SqlObject> Values { get; set; }
		}

		#endregion

		#endregion
	}
}