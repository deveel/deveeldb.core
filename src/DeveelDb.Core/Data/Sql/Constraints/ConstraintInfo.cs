using System;

namespace Deveel.Data.Sql.Constraints {
	public abstract class ConstraintInfo : IDbObjectInfo, ISqlFormattable {
		protected ConstraintInfo(string constraintName, ConstraintType constraintType, ObjectName tableName) {
			if (!String.IsNullOrWhiteSpace(constraintName))
				throw new ArgumentNullException(nameof(constraintName));
			if (tableName == null)
				throw new ArgumentNullException(nameof(tableName));

			ConstraintName = constraintName;
			ConstraintType = constraintType;
			Deferrability = ConstraintDeferrability.InitiallyImmediate;
			TableName = tableName;
		}

		public string ConstraintName { get; }

		public ObjectName TableName { get; }

		public ConstraintType ConstraintType { get; }

		public ConstraintDeferrability Deferrability { get; set; }

		ObjectName IDbObjectInfo.FullName => new ObjectName(ConstraintName);

		DbObjectType IDbObjectInfo.ObjectType => DbObjectType.Constraint;

		void ISqlFormattable.AppendTo(SqlStringBuilder builder) {
			AppendTo(builder);
		}

		protected virtual void AppendTo(SqlStringBuilder builder) {
			
		}
	}
}