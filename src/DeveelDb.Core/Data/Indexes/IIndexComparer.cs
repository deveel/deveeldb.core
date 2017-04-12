using System;

namespace Deveel.Data.Indexes {
	public interface IIndexComparer<TKey, TValue> {
		int Compare(TValue indexed, TKey key);
	}
}