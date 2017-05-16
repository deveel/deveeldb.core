using System;

using Deveel.Data.Sql;

namespace Deveel.Data {
	public static class DefaultSystemErrors {
		public static class System {
			public static readonly SystemError Unknown =
				new SystemError(ErrorClasses.System, ErrorCodes.System.Unknown, ErrorNames.System.Unknown);
		}
	}
}