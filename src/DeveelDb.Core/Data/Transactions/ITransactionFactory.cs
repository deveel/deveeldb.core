using System;
using System.Threading.Tasks;

namespace Deveel.Data.Transactions {
	public interface ITransactionFactory {
		Task<ITransaction> CreateTransactionAsync(IsolationLevel level);
	}
}