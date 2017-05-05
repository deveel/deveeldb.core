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

			if (String.IsNullOrWhiteSpace(authType))
				authType = database.Configuration.GetString("auth.type");

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
			if (isolationLevel == null ||
				isolationLevel.Value == IsolationLevel.Unspecified)
				isolationLevel = database.Configuration.GetValue<IsolationLevel?>("transaction.isolationLevel");

			if (isolationLevel == null) {
				isolationLevel = IsolationLevel.Serializable;
			} else if (isolationLevel.Value == IsolationLevel.Unspecified) {
				throw new InvalidOperationException("Invalid default isolation level for the database");
			}

			return database.CreateTransactionAsync(isolationLevel.Value);
		}

		public static async Task<ISession> CreateSessionAsync(this IDatabase database, IConfiguration configuration) {
			var user = await database.AuthenticateAsync(configuration);
			if (user == null)
				throw new InvalidOperationException();

			var transaction = await database.CreateTransaction(configuration);

			var session = new Session(database, transaction, user, configuration);

			var registry = database.Scope.Resolve<ISessionRegistry>();
			if (registry != null)
				registry.RegisterSession(session);

			return session;
		}

		public static Task<ISession> CreateSessionAsync(this IDatabase database, string userName, string password, IsolationLevel isolationLevel) {
			var config = new ConfigurationBuilder()
				.WithSetting("auth.type", "password")
				.WithSetting("auth.userName", userName)
				.WithSetting("auth.password", password)
				.WithSetting("transaction.isolationLevel", isolationLevel);

			return database.CreateSessionAsync(config.Build());
		}

		internal static async Task<ISession> CreateSystemSessionAsync(this IDatabase database, IsolationLevel isolationLevel = IsolationLevel.Serializable) {
			var transaction = await database.CreateTransactionAsync(isolationLevel);
			return new Session(database, transaction, User.System, new Configuration.Configuration());
		}
	}
}