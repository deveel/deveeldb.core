using System;

namespace Deveel.Data.Sql {
	public class SqlException : Exception {
		public SqlException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public SqlException(string message)
			: base(message) {
		}

		public SqlException() {
		}
	}
}