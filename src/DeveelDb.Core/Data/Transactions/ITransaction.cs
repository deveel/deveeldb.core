using System;

namespace Deveel.Data.Transactions {
	public interface ITransaction : IContext {
		IDatabase Database { get; }

		IsolationLevel IsolationLevel { get; }


		void Commit(string savePointName);

		void Rollback();
	}
}