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

using Deveel.Data.Services;

namespace Deveel.Data.Configuration {
	public static class ScopeExtensions {
		public static void SetConfiguration(this IScope scope, IConfiguration configuration) {
			var config = scope.Resolve<IConfiguration>();
			var final = configuration;
			if (config != null)
				final = final.MergeWith(configuration);

			scope.Unregister<IConfiguration>();
			scope.RegisterInstance<IConfiguration>(final);
		}

		public static void AddConfigurationFormatter<TFormatter>(this IScope scope, string name)
			where TFormatter : class, IConfigurationFormatter {
			scope.Register<TFormatter>(name);
		}
	}
}