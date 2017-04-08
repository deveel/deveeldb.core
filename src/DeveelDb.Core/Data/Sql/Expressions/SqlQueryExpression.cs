using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Deveel.Data.Sql.Expressions {
	public sealed class SqlQueryExpression : SqlExpression {
		public SqlQueryExpression()
			: base(SqlExpressionType.Query) {
			From = new SqlQueryExpressionFrom();
			GroupBy = new List<SqlExpression>();
			Items = new ItemList();
		}

		public IList<SqlQueryExpressionItem> Items { get; }

		public bool All {
			get { return Items.Count == 1 && Items[0].IsAll; }
			set {
				if (value) {
					Items.Add(SqlQueryExpressionItem.All);
				}
			}
		}

		public SqlQueryExpressionFrom From { get; set; }

		public override bool CanReduce => true;

		public bool Distinct { get; set; }

		public SqlExpression Where { get; set; }

		public SqlExpression Having { get; set; }

		public IList<SqlExpression> GroupBy { get; set; }

		public ObjectName GroupMax { get; set; }

		public override SqlExpression Accept(SqlExpressionVisitor visitor) {
			return visitor.VisitQuery(this);
		}

		protected override void AppendTo(SqlStringBuilder builder) {
			builder.Append("SELECT ");
			if (Distinct)
				builder.Append("DISTINCT ");

			for (int i = 0; i < Items.Count; i++) {
				Items[i].AppendTo(builder);

				if (i < Items.Count - 1)
					builder.Append(", ");
			}

			if (!From.IsEmpty) {
				builder.Append(" ");
				From.AppendTo(builder);
			}

			// TODO: continue
		}

		#region ItemList

		class ItemList : Collection<SqlQueryExpressionItem> {
			protected override void InsertItem(int index, SqlQueryExpressionItem item) {
				if (item.IsAll && Items.Any(x => x.IsAll))
					throw new ArgumentException("A query expression cannot contain more than one ALL item");

				base.InsertItem(index, item);
			}

			protected override void SetItem(int index, SqlQueryExpressionItem item) {
				if (item.IsAll) {
					var other = Items[index];
					if (!other.IsAll)
						throw new ArgumentException("Trying to set an ALL item in a query that has already one.");
				}

				base.SetItem(index, item);
			}

			protected override void RemoveItem(int index) {
				if (Items.Count - 1 == 0)
					throw new InvalidOperationException("Cannot remove the last item of a select list");

				base.RemoveItem(index);
			}
		}

		#endregion
	}
}