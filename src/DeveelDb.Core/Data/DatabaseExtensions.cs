using System;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Configuration;
using Deveel.Data.Security;
using Deveel.Data.Services;
using Deveel.Data.Transactions;

namespace Deveel.Data {
	public static class DatabaseExtensions {
		private static Task<User> AuthenticateAsync(this IDatabase database, IConfiguration configuration) {
			var authType = configuration.GetString("auth.type");

			IAuthenticator authenticator;

			if (!String.IsNullOrWhiteSpace(authType)) {
				authenticator = database.Scope.Resolve<IAuthenticator>(authType);
			} else {
				authenticator = database.Scope.ResolveAll<IAuthenticator>()
					.FirstOrDefault();
			}

			if (authenticator != null)
				return authenticator.AuthenticateAsync(configuration);

			throw new InvalidOperationException();
		}

		private static Task<ITransaction> CreateTransaction(this IDatabase database, IConfiguration configuration) {
			var isolationLevel = configuration.GetValue<IsolationLevel?>("transaction.isolationLevel");
			if (isolationLevel == null)
				isolationLevel = database.Configuration.GetValue<IsolationLevel?>("transaction.isolationLevel");

			if (isolationLevel == null)
				isolationLevel = IsolationLevel.Serializable;

			return database.CreateTransactionAsync(isolationLevel.Value);
		}

		public static Task<ITransaction> CreateTransactionAsync(this IDatabase database, IsolationLevel level) {
			var factory = database.Scope.Resolve<ITransactionFactory>();
			if (factory == null)
				throw new InvalidOperationException();

			return factory.CreateTransactionAsync(level);
		}

		public static async Task<ISession> CreateSession(this IDatabase database, IConfiguration configuration) {
			var user = await database.AuthenticateAsync(configuration);
			if (user == null)
				throw new InvalidOperationException();

			var transaction = await database.CreateTransaction(configuration);
			return new Session(database, transaction, user, configuration);
		}
	}
}