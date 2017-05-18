namespace Deveel.Data.Sql.Tables.Infrastructure {
	class TableSourceGC {
		private ITableSource source;
		private IContext context;

		public TableSourceGC(IContext context, ITableSource source) {
			this.context = context;
			this.source = source;
		}
	}
}