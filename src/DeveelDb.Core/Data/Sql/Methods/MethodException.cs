using System;

namespace Deveel.Data.Sql.Methods {
	public class MethodException : SqlException {
		public MethodException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public MethodException(string message)
			: base(message) {
		}

		public MethodException() {
		}
	}
}