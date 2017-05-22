using System;

using Deveel.Data.Services;
using Deveel.Data.Sql.Variables;

namespace Deveel.Data.Sql.Statements {
	public class BlockStatementContext : StatementContext, IVariableScope {
		public BlockStatementContext(IContext parent, SqlStatement statement) 
			: base(parent, statement) {
			Init();
		}

		public BlockStatementContext(IContext parent, SqlStatement statement, Action<IScope> scopeInit) 
			: base(parent, statement, scopeInit) {
			Init();
		}

		public BlockStatementContext(IContext parent, string name, SqlStatement statement) 
			: base(parent, name, statement) {
			Init();
		}

		public BlockStatementContext(IContext parent, string name, SqlStatement statement, Action<IScope> scopeInit) 
			: base(parent, name, statement, scopeInit) {
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