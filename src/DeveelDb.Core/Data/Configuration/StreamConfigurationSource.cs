using System;
using System.IO;

namespace Deveel.Data.Configuration {
	public sealed class StreamConfigurationSource : IConfigurationSource {
		public StreamConfigurationSource(Stream stream) {
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			Stream = stream;
		}

		public Stream Stream { get; }

		Stream IConfigurationSource.InputStream => Stream;
	}
}