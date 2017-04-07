using System;

namespace Deveel.Data.Sql.Variables {
	public class VariableException : SqlException {
		public VariableException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public VariableException(string message)
			: base(message) {
		}

		public VariableException() {
		}
	}
}