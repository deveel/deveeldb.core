// 
//  Copyright 2010-2016 Deveel
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Deveel.Data;

namespace Deveel.Data.Configuration {
	public class Configuration : IConfiguration {
		private readonly Dictionary<string, object> values;
		private readonly Dictionary<string, IConfiguration> childConfigurations;

		/// <summary>
		/// A character that separates sections in a configuration context
		/// </summary>
		public const char SectionSeparator = '.';

		/// <summary>
		/// Constructs the <see cref="Configuration"/>.
		/// </summary>
		public Configuration() {
			values = new Dictionary<string, object>();
			childConfigurations = new Dictionary<string, IConfiguration>();
		}

		/// <inheritdoc/>
		public IConfiguration Parent { get; set; }

		/// <inheritdoc/>
		public IEnumerable<string> Keys {
			get { return values.Keys; }
		}

		/// <inheritdoc/>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
			return values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}


		/// <inheritdoc/>
		public void SetValue(string key, object value) {
			if (String.IsNullOrEmpty(key))
				throw new ArgumentNullException(nameof(key));

			var parts = key.Split(SectionSeparator);
			if (parts.Length == 0)
				throw new ArgumentException();

			if (parts.Length == 1) {
				if (value == null) {
					values.Remove(key);
				} else {
					values[key] = value;
				}
			} else {
				IConfiguration config = this;
				for (int i = 0; i < parts.Length; i++) {
					var part = parts[i];
					if (i == parts.Length - 1) {
						config.SetValue(part, value);
						return;
					}

					var child = config.GetChild(part);
					if (child == null) {
						child = new Configuration();
						config.AddChild(part, child);
					}

					config = child;
				}
			}
		}

		/// <inheritdoc/>
		public object GetValue(string key) {
			if (String.IsNullOrEmpty(key))
				throw new ArgumentNullException(nameof(key));

			var parts = key.Split(SectionSeparator);
			if (parts.Length == 0)
				throw new ArgumentException();

			if (parts.Length == 1) {
				object value;
				if (values.TryGetValue(key, out value))
					return value;

				return null;
			}

			IConfiguration config = this;
			for (int i = 0; i < parts.Length; i++) {
				var part = parts[i];
				if (i == parts.Length - 1)
					return config.GetValue(part);

				config = config.GetChild(part);

				if (config == null)
					return null;
			}

			return null;
		}

		/// <inheritdoc cref="IConfiguration.AddChild"/>
		public void AddChild(string key, IConfiguration configuration) {
			if (String.IsNullOrEmpty(key))
				throw new ArgumentNullException(nameof(key));
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			childConfigurations[key] = configuration;
		}

		/// <inheritdoc cref="IConfiguration.GetChildren"/>
		public IEnumerable<KeyValuePair<string, IConfiguration>> GetChildren() {
			return childConfigurations.AsEnumerable();
		}

		public static IConfiguration Build(Action<IConfigurationBuilder> config) {
			var builder = new ConfigurationBuilder();
			config(builder);
			return builder.Build();
		}

		public static IConfigurationBuilder Builder() {
			return new ConfigurationBuilder();
		}
	}
}