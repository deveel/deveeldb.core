using System;

namespace Deveel.Data {
	public interface IDatabase : IContext {
		string Name { get; }
	}
}