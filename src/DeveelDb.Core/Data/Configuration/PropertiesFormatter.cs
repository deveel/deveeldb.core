using System;
using System.IO;

using Deveel.Data.Configuration.Util;

namespace Deveel.Data.Configuration {
	public sealed class PropertiesFormatter : IConfigurationFormatter {
		void IConfigurationFormatter.LoadInto(IConfigurationBuilder config, Stream inputStream) {
			var properties = new Properties();
			properties.Load(inputStream);

			foreach (var key in properties.Keys) {
				string value;
				if (properties.TryGetValue(key, out value)) 
					config.WithSetting(key, value);
			}
		}
	}
}