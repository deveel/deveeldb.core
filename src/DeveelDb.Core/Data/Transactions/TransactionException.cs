using System;

namespace Deveel.Data.Transactions {
	public class TransactionException : Exception {
		public TransactionException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public TransactionException(string message)
			: base(message) {
		}

		public TransactionException() {
		}
	}
}