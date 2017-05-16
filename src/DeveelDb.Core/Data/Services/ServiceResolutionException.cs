// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;

using Deveel.Data.Sql;

namespace Deveel.Data.Services {
	/// <summary>
	/// The exception thrown during the resolution of a service
	/// within a <see cref="IScope"/>.
	/// </summary>
	public class ServiceResolutionException : ServiceException {
		public ServiceResolutionException(Type serviceType, string message, Exception innerException)
			: base(ErrorCodes.Services.NotResolved, ResolveMessage(ErrorClasses.Services, 
				ErrorCodes.Services.NotResolved, message, serviceType), innerException) {
			ServiceType = serviceType;
		}

		public ServiceResolutionException(Type serviceType, string message)
			: this(serviceType, message, null) {
		}

		public ServiceResolutionException(Type serviceType)
			: this(serviceType, null) {
		}

		/// <summary>
		/// Gets the type of the service that caused issues during resolution
		/// </summary>
		public Type ServiceType { get; }
	}
}