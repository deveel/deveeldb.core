using System;

namespace Deveel.Data.Sql {
	public interface IGroupResolver {
		long Size { get; }

		SqlObject ResolveReference(ObjectName reference, long index);

		IReferenceResolver GetResolver(long index);
	}
}