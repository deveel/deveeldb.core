using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Deveel.Data.Configuration {
	public sealed class StringConfigurationSource : IConfigurationSource {
		public StringConfigurationSource(string source) {
			Source = source;
		}

		public string Source { get; }

		Stream IConfigurationSource.InputStream {
			get {
				var bytes = Encoding.UTF8.GetBytes(Source);
				return new MemoryStream(bytes);
			}
		}
	}
}