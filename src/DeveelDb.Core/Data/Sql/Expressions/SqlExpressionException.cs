using System;

namespace Deveel.Data.Sql.Expressions {
	public class SqlExpressionException : Exception {
		public SqlExpressionException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public SqlExpressionException(string message)
			: base(message) {
		}

		public SqlExpressionException() {
		}
	}
}