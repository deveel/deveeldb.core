using System;

namespace Deveel.Data {
	public class DatabaseSystemException : Exception {
		public DatabaseSystemException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public DatabaseSystemException(string message)
			: base(message) {
		}

		public DatabaseSystemException() {
		}
	}
}