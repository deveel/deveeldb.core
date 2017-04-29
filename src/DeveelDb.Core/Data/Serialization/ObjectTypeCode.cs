using System;

namespace Deveel.Data.Serialization {
	enum ObjectTypeCode {
		Primitive = 1,
		Enum,
		Object,
		Serializable,
		Array,
		List,
		Dictionary
	}
}