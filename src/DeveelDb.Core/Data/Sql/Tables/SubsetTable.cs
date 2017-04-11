using System;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables {
	public class SubsetTable : FilterTable, IRootTable {
		private readonly int[] columns;
		private readonly ObjectName[] aliases;
		private readonly TableInfo tableInfo;

		public SubsetTable(ITable table, int[] columns, ObjectName[] aliases)
			: base(table) {
			if (columns.Length != aliases.Length)
				throw new ArgumentException();

			this.columns = columns;
			this.aliases = aliases;

			var parentInfo = Parent.TableInfo;
			tableInfo = new TableInfo(parentInfo.TableName);

			for (int i = 0; i < columns.Length; ++i) {
				int mapTo = columns[i];

				var origColumnInfo = Parent.TableInfo.Columns[mapTo];
				var columnInfo = new ColumnInfo(aliases[i].Name, origColumnInfo.ColumnType) {
					DefaultValue = origColumnInfo.DefaultValue
				};

				tableInfo.Columns.Add(columnInfo);
			}

			tableInfo = TableInfo.ReadOnly(tableInfo);
		}

		public override TableInfo TableInfo => tableInfo;

		protected override IEnumerable<long> ResolveRows(int column, IEnumerable<long> rows, ITable ancestor) {
			return base.ResolveRows(columns[column], rows, ancestor);
		}

		public override SqlObject GetValue(long row, int column) {
			return base.GetValue(row, columns[column]);
		}

		bool IEquatable<ITable>.Equals(ITable other) {
			return this == other;
		}
	}
}