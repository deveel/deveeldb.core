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

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Expressions;

namespace Deveel.Data.Sql.Statements {
	public sealed class ContinueStatement : LoopControlStatement {
		public ContinueStatement() : base(LoopControlType.Continue) {
		}

		public ContinueStatement(SqlExpression when) : base(LoopControlType.Continue, when) {
		}

		public ContinueStatement(string label) : base(LoopControlType.Continue, label) {
		}

		public ContinueStatement(string label, SqlExpression when) : base(LoopControlType.Continue, label, when) {
		}

		private ContinueStatement(SerializationInfo info) : base(info) {
		}
	}
}