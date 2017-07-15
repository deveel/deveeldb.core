using System;

using Deveel.Data.Sql;
using Deveel.Data.Storage;

namespace Deveel.Data.Indexes {
	public interface IMappedBlock : IIndexBlock<SqlObject, long> {
		long FirstEntry { get; }

		long LastEntry { get; }

		long BlockPointer { get; }

		byte CompactType { get; }


		long CopyTo(IStore destStore);

		long Flush();
	}
}