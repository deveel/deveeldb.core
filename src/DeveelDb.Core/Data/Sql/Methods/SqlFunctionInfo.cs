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

using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Methods {
	public sealed class SqlFunctionInfo : SqlMethodInfo {
		public SqlFunctionInfo(ObjectName functionName, SqlType returnType)
			: this(functionName, FunctionType.Scalar, returnType) {
		}

		public SqlFunctionInfo(ObjectName functionName, FunctionType functionType, SqlType returnType)
			: base(functionName, MethodType.Function) {
			if (returnType == null)
				throw new ArgumentNullException(nameof(returnType));

			FunctionType = functionType;
			ReturnType = returnType;
		}

		public FunctionType FunctionType { get; }

		public SqlType ReturnType { get; }

		internal override void AppendTo(SqlStringBuilder builder) {
			builder.Append(Type.ToString().ToUpperInvariant());
			builder.Append(" ");
			MethodName.AppendTo(builder);

			AppendParametersTo(builder);

			builder.Append(" RETURNS ");
			ReturnType.AppendTo(builder);
		}
	}
}