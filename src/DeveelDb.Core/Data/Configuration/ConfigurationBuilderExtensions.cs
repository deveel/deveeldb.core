using System;
using System.IO;

namespace Deveel.Data.Configuration {
	public static class ConfigurationBuilderExtensions {
		public static IConfigurationBuilder Add(this IConfigurationBuilder builder, IConfigurationSource source, IConfigurationFormatter formatter) {
			using (var stream = source.InputStream) {
				formatter.LoadInto(builder, stream);
			}

			return builder;
		}

		public static IConfigurationBuilder AddProperties(this IConfigurationBuilder builder, IConfigurationSource source)
			=> builder.Add(source, new PropertiesFormatter());

		public static IConfigurationBuilder AddPropertiesString(this IConfigurationBuilder builder, string source)
			=> builder.AddProperties(new StringConfigurationSource(source));

		public static IConfigurationBuilder AddPropertiesFile(this IConfigurationBuilder builder, string fileName)
			=> builder.AddProperties(new FileConfigurationSource(fileName));

		public static IConfigurationBuilder AddPropertiesStream(this IConfigurationBuilder builder, Stream stream)
			=> builder.AddProperties(new StreamConfigurationSource(stream));
	}
}