using System;

namespace Deveel.Data.Sql {
	public static class ErrorCodes {
		public static class System {
			public const int Unknown = 001;
		}

		public static class Configuration {
			public const int RequiredKeyMissing = 105;
		}

		public static class Services {
			public const int Unknown = 001;
			public const int NotResolved = 101;
			public const int KeyNotFound = 102;
		}

		public static class SqlModel {
			public const int Unknown = 001;

			public static class Expression {
				public const int Unknown = 010;
			}
		}

		public static class Security {
			public const int Unknown = 001;
		}
	}
}