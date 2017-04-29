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

using Deveel.Data.Serialization;
using Deveel.Data.Services;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlReferenceExpression : SqlExpression {
		internal SqlReferenceExpression(ObjectName reference)
			: base(SqlExpressionType.Reference) {
			if (reference == null)
				throw new ArgumentNullException(nameof(reference));

			ReferenceName = reference;
		}

		private SqlReferenceExpression(SerializationInfo info)
			: base(info) {
			ReferenceName = info.GetValue<ObjectName>("ref");
		}

		public ObjectName ReferenceName { get; }

		public override bool IsReference => true;

		protected override void AppendTo(SqlStringBuilder builder) {
			ReferenceName.AppendTo(builder);
		}

		protected override void GetObjectData(SerializationInfo info) {
			info.SetValue("ref", ReferenceName);
		}

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitReference(this);
		}

		public override bool CanReduce => true;

		public override async Task<SqlExpression> ReduceAsync(IContext context) {
			if (context == null)
				throw new SqlExpressionException("A reference cannot be reduced outside a context.");

			var resolver = context.GetReferenceResolver();
			if (resolver == null)
				throw new SqlExpressionException("No reference resolver was declared in this scope");

			var value = await resolver.ResolveReferenceAsync(ReferenceName);
			if (value == null)
				value = SqlObject.Unknown;

			return Constant(value);
		}

		public override SqlType GetSqlType(IContext context) {
			if (context == null)
				throw new SqlExpressionException("A reference cannot be reduced outside a context.");

			var resolver = context.GetReferenceResolver();
			if (resolver == null)
				throw new SqlExpressionException("No reference resolver was declared in this scope");

			var value = resolver.ResolveType(ReferenceName);
			if (value == null)
				throw new SqlExpressionException();

			return value;
		}
	}
}