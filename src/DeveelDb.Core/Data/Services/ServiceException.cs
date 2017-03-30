using System;

namespace Deveel.Data.Services {
	/// <summary>
	/// An exception thrown within the services domain
	/// </summary>
	public class ServiceException : Exception {
		public ServiceException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public ServiceException(string message)
			: base(message) {
		}

		public ServiceException() {
		}
	}
}