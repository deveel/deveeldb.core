using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Types {
	public sealed class SqlTypeResolveInfo {
		public SqlTypeResolveInfo(string typeName, IDictionary<string, object> properties) {
			if (String.IsNullOrEmpty(typeName))
				throw new ArgumentNullException(nameof(typeName));

			if (properties == null)
				properties = new Dictionary<string, object>();

			TypeName = typeName;
			Properties = properties;
		}

		public SqlTypeResolveInfo(string typeName)
			: this(typeName, new Dictionary<string, object>()) {
		}

		public string TypeName { get; }

		public IDictionary<string, object> Properties { get; }
	}
}