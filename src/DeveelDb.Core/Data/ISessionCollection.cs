using System;
using System.Collections.Generic;

namespace Deveel.Data {
	public interface ISessionCollection : IEnumerable<ISession> {
		int Count { get; }

		ISession this[int index] { get; }
	}
}