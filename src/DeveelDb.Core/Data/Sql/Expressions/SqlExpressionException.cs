﻿// 
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

namespace Deveel.Data.Sql.Expressions {
	public class SqlExpressionException : SqlException {
		public SqlExpressionException(string message, Exception innerException)
			: this(ErrorCodes.SqlModel.Expression.Unknown, message, innerException) {
		}

		public SqlExpressionException(int code, string message, Exception innerException)
			: base(code, message, innerException) {
		}

		public SqlExpressionException(string message)
			: this(ErrorCodes.SqlModel.Expression.Unknown, message) {
		}

		public SqlExpressionException(int code, string message)
			: base(code, message) {
		}

		public SqlExpressionException()
			: this(ErrorCodes.SqlModel.Expression.Unknown) {
		}

		public SqlExpressionException(int code)
			: base(code) {
		}
	}
}