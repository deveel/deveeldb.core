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
using System.IO;
using System.Linq;
using System.Reflection;

using Deveel.Data.Configuration;
using Deveel.Data.Services;

using DryIoc;

using IScope = Deveel.Data.Services.IScope;

namespace Deveel.Data {
	public static class SystemBuilderExtensions {
		public static ISystemBuilder ConfigureServices(this ISystemBuilder builder, Action<IScope> configure) {
			return builder.ConfigureServices((context, scope) => configure(scope));
		}

		public static ISystemBuilder UseConfiguration(this ISystemBuilder builder, IConfiguration configuration) {
			return builder.ConfigureServices(services => services.ReplaceInstance<IConfiguration>(configuration));
		}

		public static ISystemBuilder UseRootPath(this ISystemBuilder builder, string path) {
			return builder.UseSetting("rootPath", path);
		}

		public static ISystemBuilder UseSystemServices(this ISystemBuilder builder, Assembly assembly) {
			return builder.ConfigureServices(scope => {
				var types = assembly.GetImplementationTypes()
					.Where(x => typeof(ISystemServicesProvider).GetTypeInfo().IsAssignableFrom(x.GetTypeInfo()));

				foreach (var type in types) {
					scope.Register(typeof(ISystemServicesProvider), type, null);
				}
			});
		}

		public static ISystemBuilder UseSystemServices(this ISystemBuilder builder)
			=> builder.UseSystemServices(typeof(DatabaseSystem).GetTypeInfo().Assembly);
	}
}