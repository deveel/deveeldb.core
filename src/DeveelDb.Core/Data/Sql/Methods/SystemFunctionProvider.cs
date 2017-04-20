using System;
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
			SqlParameterInfo param,
			SqlType returnType,
			Action<IterateContext> iterate,
			Func<InitializeContext, Task> initialize = null,
			Func<MergeContext, Task> merge = null) {
			var methodInfo = new SqlFunctionInfo(new ObjectName(name), returnType);
			methodInfo.Parameters.Add(param);

			var aggregate = new SqlAggregateFunctionDelegate(methodInfo, iterate);
			aggregate.Seed(initialize);
			aggregate.Aggregate(merge);


			Register(aggregate);
		}


		private void RegisterAggregates() {
			// COUNT(*)
			RegisterAggregate("COUNT", Deterministic("x"), PrimitiveTypes.VarNumeric(),
				iterate => {
					if (iterate.IsFirst) {
						iterate.SetResult(iterate.Current);
					} else {
						iterate.SetResult(iterate.Current.Add(iterate.Accumulation));
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

			RegisterAggregate("MIN", Deterministic("x"), PrimitiveTypes.VarNumeric(),
				iterate => {
					if (iterate.IsFirst) {
						iterate.SetResult(iterate.Current);
					} else if (iterate.Current.LessThan(iterate.Accumulation).IsTrue) {
						iterate.SetResult(iterate.Current);
					}
				});

			RegisterAggregate("MAX", Deterministic("x"), PrimitiveTypes.VarNumeric(),
				iterate => {
					if (iterate.IsFirst) {
						iterate.SetResult(iterate.Current);
					} else if (iterate.Current.GreaterThan(iterate.Accumulation).IsTrue) {
						iterate.SetResult(iterate.Current);
					} else if (iterate.Accumulation.LessThan(iterate.Current).IsTrue) {
						iterate.SetResult(iterate.Accumulation);
					}
				});

			RegisterAggregate("AVG", Deterministic("x"), PrimitiveTypes.VarNumeric(),
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
		}

		#endregion
	}
}