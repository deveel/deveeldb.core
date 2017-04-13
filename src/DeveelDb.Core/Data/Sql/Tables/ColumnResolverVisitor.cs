using System;

using Deveel.Data.Sql.Tables;

namespace Deveel.Data.Sql.Expressions
{
    class ColumnResolverVisitor : SqlExpressionVisitor {
	    private readonly TableInfo tableInfo;
	    private readonly bool ignoreCase;

	    public ColumnResolverVisitor(TableInfo tableInfo, bool ignoreCase) {
		    this.tableInfo = tableInfo;
		    this.ignoreCase = ignoreCase;
	    }

	    public override SqlExpression VisitReference(SqlReferenceExpression expression) {
		    if (expression.ReferenceName.Parent == null) {
			    var columnName = expression.ReferenceName.Name;
			    var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			    foreach (var column in tableInfo.Columns) {
				    if (String.Equals(column.ColumnName, columnName, comparison)) {
					    return SqlExpression.Reference(new ObjectName(tableInfo.TableName, column.ColumnName));
				    }
			    }
		    }
		    return base.VisitReference(expression);
	    }
    }
}
