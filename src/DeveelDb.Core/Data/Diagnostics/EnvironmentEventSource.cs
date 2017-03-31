using System;
using System.Collections.Generic;

namespace Deveel.Data.Diagnostics {
	public sealed class EnvironmentEventSource : EventSource {
		protected override void GetMetadata(Dictionary<string, object> metadata) {
			var variables = Environment.GetEnvironmentVariables();
			foreach (var variable in variables.Keys) {
				var value = variables[variable];

				var key = String.Format("env.{0}", variable);
				metadata[key] = value;
			}
		}
	}
}