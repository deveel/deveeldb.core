using System;

namespace Deveel.Data.Services {
	/// <summary>
	/// The exception thrown during the resolution of a service
	/// within a <see cref="IScope"/>.
	/// </summary>
	public class ServiceResolutionException : ServiceException {
		public ServiceResolutionException(Type serviceType, string message, Exception innerException)
			: base(message, innerException) {
			ServiceType = serviceType;
		}

		public ServiceResolutionException(Type serviceType, string message)
			: this(serviceType, message, null) {
		}

		public ServiceResolutionException(Type serviceType)
			: this(serviceType, $"An error occurred while trying to resolve a service of typ '{serviceType}") {
		}

		/// <summary>
		/// Gets the type of the service that caused issues during resolution
		/// </summary>
		public Type ServiceType { get; }
	}
}