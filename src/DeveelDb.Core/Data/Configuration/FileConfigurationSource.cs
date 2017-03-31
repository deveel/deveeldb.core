using System;
using System.IO;

namespace Deveel.Data.Configuration {
	public sealed class FileConfigurationSource : IConfigurationSource {
		public FileConfigurationSource(string fileName) {
			if (String.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			FileName = fileName;
		}

		public string FileName { get; }

		Stream IConfigurationSource.InputStream => new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
	}
}