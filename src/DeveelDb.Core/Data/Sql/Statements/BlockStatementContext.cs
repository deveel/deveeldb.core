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

using Deveel.Data.Services;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Statements {
	public class BlockStatementContext : StatementContext, IVariableScope {
		public BlockStatementContext(IContext parent, SqlStatement statement) 
			: base(parent, statement) {
			Init();
		}

		public BlockStatementContext(IContext parent, string name, SqlStatement statement) 
			: base(parent, name, statement) {
			Init();
		}

		public VariableManager Variables { get; private set; }

		IVariableManager IVariableScope.Variables => Variables;

		private void Init() {
			Variables = new VariableManager();
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (Variables != null)
					Variables.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}