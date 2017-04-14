using System;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlQueryExpressionSource : ISqlExpressionPreparable<SqlQueryExpressionSource>, ISqlFormattable {
		private SqlQueryExpressionSource(ObjectName tableName, SqlQueryExpression query, string alias) {
			TableName = tableName;
			Query = query;
			Alias = alias;
		}

		public SqlQueryExpressionSource(ObjectName tableName, string alias)
			: this(tableName, null, alias) {
			if (tableName == null)
				throw new ArgumentException(nameof(tableName));
		}

		public SqlQueryExpressionSource(SqlQueryExpression query, string alias)
			: this(null, query, alias) {
			if (query == null)
				throw new ArgumentNullException(nameof(query));
		}

		public string Alias { get; }

		public bool IsAliased => !String.IsNullOrWhiteSpace(Alias);

		public ObjectName TableName { get; }

		public SqlQueryExpression Query { get; }

		public bool IsQuery => Query != null;

		public bool IsTable => TableName != null;

		internal string UniqueKey { get; set; }

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			if (IsTable) {
				TableName.AppendTo(builder);
			} else {
				builder.Append("(");
				Query.AppendTo(builder);
				builder.Append(")");
			}

			if (IsAliased) {
				builder.AppendFormat(" AS {0}", Alias);
			}
		}

		public override string ToString() {
			return this.ToSqlString();
		}

		SqlQueryExpressionSource ISqlExpressionPreparable<SqlQueryExpressionSource>.PrepareExpressions(ISqlExpressionPreparer preparer) {
			var query = Query;
			if (query != null)
				query = (SqlQueryExpression) query.Prepare(preparer);

			return new SqlQueryExpressionSource(TableName, query, Alias);
		}
	}
}