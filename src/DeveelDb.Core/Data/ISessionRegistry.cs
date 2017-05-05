using System;

namespace Deveel.Data {
	public interface ISessionRegistry : ISessionCollection {
		void RegisterSession(ISession session);

		void UnregisterSession(ISession session);
	}
}