using System;

namespace Deveel.Data.Sql.Tables {
	public interface ITableCache : IDisposable {
		bool TryGetValue(FieldId fieldId, out SqlObject value);

		void SetValue(FieldId fieldId, SqlObject value);

		bool Remove(FieldId fieldId);

		void Clear();
	}
}