using System;

namespace Deveel.Data {
	public sealed class SystemError {
		public SystemError(int @class, int code, string name) {
			Class = @class;
			Code = code;
			Name = name;
		}

		public int Code { get; }

		public int Class { get; }

		public string Name { get; }

		public SystemException AsException() {
			return AsException(null);
		}

		public SystemException AsException(string message) {
			return new SystemException(Class, Code, message);
		}

		public override string ToString() {
			return $"DDB-{Class:D3}{Code:D3}";
		}

		internal static string Message(int @class, int code, string message, params object[] args) {
			if (!String.IsNullOrWhiteSpace(message))
				return String.Format(message, args);

			var format = ErrorMessages.ResourceManager.GetString($"Error_{@class:D3}{code:D3}");
			return String.Format(format, args);
		}
	}
}