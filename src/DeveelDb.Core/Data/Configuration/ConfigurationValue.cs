using System;

namespace Deveel.Data.Configuration {
	public struct ConfigurationValue {
		public ConfigurationValue(string key, object value) {
			if (String.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			Key = key;
			Value = value;
		}

		public string Key { get; }

		public object Value { get; }
	}
}