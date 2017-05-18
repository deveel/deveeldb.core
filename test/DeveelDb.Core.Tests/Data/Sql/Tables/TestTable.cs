using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Deveel.Data.Serialization;
using Deveel.Data.Sql.Indexes;

namespace Deveel.Data.Sql.Tables {
	class TestTable : IRootTable {
		private readonly IList<SqlObject[]> rows;
		private readonly TableInfo tableInfo;

		public TestTable(TableInfo tableInfo, IList<SqlObject[]> rows) {
			this.tableInfo = tableInfo;
			this.rows = rows;
		}

		IDbObjectInfo IDbObject.ObjectInfo => tableInfo;

		TableInfo ITable.TableInfo => tableInfo;

		int IComparable.CompareTo(object obj) {
			throw new NotSupportedException();
		}

		int IComparable<ISqlValue>.CompareTo(ISqlValue other) {
			throw new NotSupportedException();
		}

		bool ISqlValue.IsComparableTo(ISqlValue other) {
			return false;
		}

		public IEnumerator<Row> GetEnumerator() {
			return rows.Select((values, index) => new Row(this, index)).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public long RowCount => rows.Count;

		public Task<SqlObject> GetValueAsync(long row, int column) {
			var objRow = rows[(int) row];
			return Task.FromResult(objRow[column]);
		}

		bool IEquatable<ITable>.Equals(ITable other) {
			return this == other;
		}

		public Index GetColumnIndex(int column) {
			throw new NotImplementedException();
		}

		void ISerializable.GetObjectData(SerializationInfo info) {
			throw new NotSupportedException();
		}
	}
}