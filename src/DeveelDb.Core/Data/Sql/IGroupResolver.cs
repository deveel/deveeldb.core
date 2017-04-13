using System;

namespace Deveel.Data.Sql {
	public interface IGroupResolver {
		long Size { get; }

		SqlObject ResolveReference(ObjectName referecne, long index);

		IReferenceResolver GetResolver(long index);
	}
}