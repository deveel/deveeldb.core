using System;

namespace Deveel.Data.Sql.Types {
	public interface ISqlTypeResolver {
		SqlType Resolve(SqlTypeResolveInfo resolveInfo);
	}
}