using System;
using System.Threading.Tasks;

namespace Deveel.Data.Sql.Sequences {
	public interface ISequenceHandler {
		Task<bool> HandlesSequence(ObjectName sequenceName);
			
		Task<SqlNumber> GetCurrentValueAsync(ObjectName sequenceName);

		Task<SqlNumber> GetNextValueAsync(ObjectName sequenceName);

		Task SetCurrentValueAsync(ObjectName sequenceName, SqlNumber value);
	}
}