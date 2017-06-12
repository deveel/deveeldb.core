using System;

namespace Deveel.Data.Sql.Types {
    public interface ISqlTypeParser {
        SqlType Parse(IContext context, string s);
    }
}