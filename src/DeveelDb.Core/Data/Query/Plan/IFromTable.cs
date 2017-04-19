using System;
using System.Threading.Tasks;

namespace Deveel.Data.Query.Plan {
	interface IFromTable {
		string UniqueName { get; }

		ObjectName[] Columns { get; }
			
		bool MatchesReference(string catalog, string schema, string table);

		int ResolveColumnCount(string catalog, string schema, string table, string column);

		ObjectName ResolveColumn(string catalog, string schema, string table, string column);
	}
}