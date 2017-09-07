using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal
{
	public class FbBoolTypeMapping : BoolTypeMapping
	{
		public const string TrueLiteral = "TRUE";
		public const string FalseLiteral = "FALSE";

		public FbBoolTypeMapping()
			: base("BOOLEAN", System.Data.DbType.Boolean)
		{ }

		protected override string GenerateNonNullSqlLiteral(object value)
		{
			return (bool)value ? TrueLiteral : FalseLiteral;
		}
	}
}
