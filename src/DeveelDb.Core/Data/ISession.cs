using System;

using Deveel.Data.Security;
using Deveel.Data.Transactions;

namespace Deveel.Data {
	public interface ISession : IContext {
		string CurrentSchema { get; }

		IUser User { get; }

		ITransaction Transaction { get; }


		IQuery CreateQuery();
	}
}