using System;

namespace Deveel.Data.Security {
	public sealed class User : IPriviledged {
		public const string SystemName = "@SYSTEM";
		private static readonly char[] InvalidChars = "'@\"".ToCharArray();

		private User(string name, bool validate) {
			if (validate && !IsValidName(name))
				throw new ArgumentException($"User name {name} is invalid");

			Name = name;
		}

		public User(string name)
			: this(name, true) { 
		}

		public string Name { get; }

		public bool IsSystem => String.Equals(Name, SystemName, StringComparison.OrdinalIgnoreCase);

		public static User System = new User(SystemName, false);

		public static bool IsValidName(string name) {
			if (String.IsNullOrWhiteSpace(name))
				return false;

			return name.IndexOfAny(InvalidChars) < 0;
		}
	}
}