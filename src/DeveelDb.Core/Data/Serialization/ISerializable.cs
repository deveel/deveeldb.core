using System;

namespace Deveel.Data.Serialization {
	public interface ISerializable {
		void GetObjectData(SerializationInfo info);
	}
}