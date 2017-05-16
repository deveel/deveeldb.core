using System;

using Deveel.Data.Sql;

namespace Deveel.Data.Security {
	public class SecurityException : SystemException {
		public SecurityException()
			: this(ErrorCodes.Security.Unknown) {
		}

		public SecurityException(int code)
			: this(code, null) {
		}

		public SecurityException(string message)
			: this(ErrorCodes.Security.Unknown, message) {
		}

		public SecurityException(int code, string message)
			: this(code, message, null) {
		}

		public SecurityException(string message, Exception innerException)
			: this(ErrorCodes.Security.Unknown, message, innerException) {
		}

		public SecurityException(int code, string message, Exception innerException)
			: base(ErrorClasses.Security, code, message, innerException) {
		}
	}
}