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
using System.Reflection;
using System.Threading.Tasks;

using Deveel.Data.Diagnostics;
using Deveel.Data.Security;
using Deveel.Data.Serialization;
using Deveel.Data.Services;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public abstract class SqlStatement : ISqlFormattable, ISqlExpressionPreparable<SqlStatement>, ISerializable {
		public virtual bool CanPrepare => true;

		protected SqlStatement(SerializationInfo info) {
			Location = info.GetValue<LocationInfo>("location");
		}

		protected SqlStatement() {
		}

		protected virtual string Name {
			get {
				var name = GetType().Name;
				if (name.EndsWith("Statenment", StringComparison.OrdinalIgnoreCase))
					name = name.Substring(0, name.Length - 10);

				return name;
			}
		}

		internal string StatementName => Name;

		public LocationInfo Location { get; set; }

		internal SqlStatement Parent { get; set; }

		protected virtual StatementContext CreateContext(IContext parent) {
			return new StatementContext(parent, Name, this);
		}

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		protected virtual void AppendTo(SqlStringBuilder builder) {
			
		}

		internal void CollectMetadata(IDictionary<string, object> data) {
			GetMetadata(data);
		}

		protected virtual void GetMetadata(IDictionary<string, object> data) {
			
		}

		SqlStatement ISqlExpressionPreparable<SqlStatement>.Prepare(ISqlExpressionPreparer preparer) {
			return PrepareExpressions(preparer);
		}

		protected virtual SqlStatement PrepareExpressions(ISqlExpressionPreparer preparer) {
			return this;
		}

		protected virtual SqlStatement PrepareStatement(IContext context) {
			return this;
		}

		protected virtual void Require(IRequirementCollection requirements) {
			
		}

		public SqlStatement Prepare(IContext context) {
			using (var statementContext = CreateContext(context)) {
				var preparers = context.Scope.ResolveAll<ISqlExpressionPreparer>();
				var result = this;

				foreach (var preparer in preparers) {
					result = PrepareExpressions(preparer);
				}

				if (CanPrepare)
					result = result.PrepareStatement(statementContext);

				return result;
			}
		}

		private static Task HandleRequirement(IContext context, Type handlerType, object handler, Type reqType, IRequirement requirement) {
			var method = handlerType.GetRuntimeMethod("HandleRequirementAsync", new[] { typeof(IContext), reqType });
			if (method == null)
				throw new InvalidOperationException();

			try {
				return (Task)method.Invoke(handler, new object[] { context, requirement });
			} catch (TargetInvocationException e) {
				throw e.InnerException;
			}
		}

		private async Task CheckRequirements(IContext context) {
			context.Debug(-1, "Collecting security requirements");

			var registry = new RequirementCollection();
			Require(registry);

			context.Debug(-1, "Check security requirements");

			try {
				foreach (var requirement in registry) {
					var reqType = requirement.GetType();
					var handlerType = typeof(IRequirementHandler<>).MakeGenericType(reqType);

					var handlers = context.Scope.ResolveAll(handlerType);
					foreach (var handler in handlers) {
						await HandleRequirement(context, handlerType, handler, reqType, requirement);
					}
				}
			} catch (UnauthorizedAccessException ex) {
				context.Error(-93884, $"User {context.User().Name} has not enough rights to execute", ex);
				throw;
			} catch (Exception ex) {
				context.Error(-83993, "Unknown error while checking requirements", ex);
				throw;
			}
		}

		public async Task ExecuteAsync(IContext context) {
			using (var statementContext = CreateContext(context)) {
				statementContext.Information(201, "Executing statement");

				await CheckRequirements(statementContext);

				try {
					await ExecuteStatementAsync(statementContext);
				} catch (SqlStatementException ex) {
					statementContext.Error(-670393, "The statement thrown an error", ex);
					throw;
				} catch (Exception ex) {
					statementContext.Error(-1, "Could not execute the statement", ex);
					throw new SqlStatementException("Could not execute the statement because of an error", ex);
				} finally {
					statementContext.Information(202, "The statement was executed");
				}
			}
		}

		protected abstract Task ExecuteStatementAsync(StatementContext context);

		public override string ToString() {
			return this.ToSqlString();
		}

		protected virtual void GetObjectData(SerializationInfo info) {
			
		}

		void ISerializable.GetObjectData(SerializationInfo info) {
			info.SetValue("location", Location);
			GetObjectData(info);
		}
	}
}