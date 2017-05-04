using System;
using System.Threading.Tasks;

using Deveel.Data.Configuration;

namespace Deveel.Data.Security {
	public interface IAuthenticator {
		Task<User> AuthenticateAsync(IConfiguration configuration);
	}
}