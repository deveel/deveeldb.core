using System;

namespace Deveel.Data.Serialization {
	public class SerializationException : Exception {
		public SerializationException(string message, Exception innerException)
			: base(message, innerException) {
		}

		public SerializationException(string message)
			: base(message) {
		}

		public SerializationException() {
		}
	}
}