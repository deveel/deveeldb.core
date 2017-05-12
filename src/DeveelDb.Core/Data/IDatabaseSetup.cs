using System;
using System.Threading.Tasks;

namespace Deveel.Data {
	public interface IDatabaseSetup {
		Task SetupAsync(ISession session);
	}
}