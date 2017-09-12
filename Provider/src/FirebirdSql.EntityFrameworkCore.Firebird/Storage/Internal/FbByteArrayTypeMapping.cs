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
			var hex = ((byte[])value).ToHexString();
			return $"x'{hex}'";
		}
	}
}
