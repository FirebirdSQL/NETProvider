using FirebirdSql.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal
{
	public class FbByteArrayTypeMapping : ByteArrayTypeMapping
	{
		public FbByteArrayTypeMapping()
			: base("BLOB SUB_TYPE BINARY", System.Data.DbType.Binary)
		{ }

		protected override string GenerateNonNullSqlLiteral(object value)
		{
			// DUP: SqlGenerator.FormatBinary
			return string.Format("x'{0}'", ((byte[])value).ToHexString());
		}
	}
}
