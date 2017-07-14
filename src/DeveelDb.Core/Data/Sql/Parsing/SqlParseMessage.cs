using System;

using Deveel.Data.Sql.Statements;

namespace Deveel.Data.Sql.Parsing {
	public sealed class SqlParseMessage {
		public SqlParseMessage(string message, SqlParseMessageLevel level) 
			: this(null, message, level) {
		}

		public SqlParseMessage(string code, string message, SqlParseMessageLevel level) 
			: this(code, message, level, null) {
		}

		public SqlParseMessage(string message, SqlParseMessageLevel level, LocationInfo location) 
			: this(null, message, level, location) {
		}

		public SqlParseMessage(string code, string message, SqlParseMessageLevel level, LocationInfo location) {
			Code = code;
			Message = message;
			Level = level;
			Location = location;
		}

		public LocationInfo Location { get; }

		public string Message { get; }

		public string Code { get; }

		public SqlParseMessageLevel Level { get; }

		public bool HasLocation => Location != null;

		public bool HasCode => !String.IsNullOrWhiteSpace(Code);
	}
}