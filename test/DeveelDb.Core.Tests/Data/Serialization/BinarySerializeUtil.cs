using System;
using System.IO;

namespace Deveel.Data.Serialization {
	static class BinarySerializeUtil {
		public static T Serialize<T>(T obj) {
			var serializer = new BinarySerializer();
			var stream = new MemoryStream();

			serializer.Serialize(obj, stream);

			stream.Seek(0, SeekOrigin.Begin);

			return (T) serializer.Deserialize(typeof(T), stream);
		}
	}
}