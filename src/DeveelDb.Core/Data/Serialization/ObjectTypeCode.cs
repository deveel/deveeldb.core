using System;

namespace Deveel.Data.Serialization {
	enum ObjectTypeCode {
		Primitive = 1,
		Object,
		Serializable,
		Array,
		List,
		Dictionary
	}
}