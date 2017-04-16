using System;

namespace Deveel.Data.Transactions {
	public sealed class LockTimeoutException : Exception {
		internal LockTimeoutException(ObjectName objName, AccessType accessType, int timeout)
			: base($"A timeout occurred for {accessType} access on {objName} after {timeout} ms") {
			ObjectName = objName;
			AccessType = accessType;
			Timeout = timeout;
		}

		public ObjectName ObjectName { get; }

		public AccessType AccessType { get; }

		public int Timeout { get; }
	}
}