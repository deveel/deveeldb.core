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

using Deveel.Data.Sql;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Query.Plan {
	class CorrelatedReferenceExpression : SqlExpression {
		public CorrelatedReferenceExpression(ObjectName reference, int queryLevel)
			: this(reference, queryLevel, null) {
		}

		public CorrelatedReferenceExpression(ObjectName reference, int queryLevel, SqlConstantExpression value)
			: base((SqlExpressionType) 233) {
			ReferenceName = reference;
			QueryLevel = queryLevel;
			Value = value;
		}

		public ObjectName ReferenceName { get; }

		public int QueryLevel { get; }

		public SqlConstantExpression Value { get; set; }

		public override bool IsReference => true;

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return this;
		}

		public override SqlType GetSqlType(IContext context) {
			if (Value != null)
				return Value.GetSqlType(context);

			var resolver = context.GetReferenceResolver();
			if (resolver == null)
				throw new SqlExpressionException();

			return resolver.ResolveType(ReferenceName);
		}

		public override async Task<SqlExpression> ReduceAsync(IContext context) {
			if (Value != null)
				return Value;

			var resolver = context.GetReferenceResolver();
			if (resolver == null)
				throw new SqlExpressionException();

			var value = await resolver.ResolveReferenceAsync(ReferenceName);
			return Constant(value);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			ReferenceName.AppendTo(builder);
			builder.AppendFormat("({0})", QueryLevel);
		}
	}
}