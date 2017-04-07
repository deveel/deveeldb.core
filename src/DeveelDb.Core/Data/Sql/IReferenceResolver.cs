using System;

namespace Deveel.Data.Sql {
	public interface IReferenceResolver {
		SqlObject ResolveReference(ObjectName referenceName);
	}
}