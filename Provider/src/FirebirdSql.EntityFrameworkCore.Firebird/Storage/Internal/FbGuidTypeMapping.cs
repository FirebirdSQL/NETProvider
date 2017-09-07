using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal
{
	public class FbGuidTypeMapping : GuidTypeMapping
	{
		public FbGuidTypeMapping()
			: base("CHAR(16) CHARACTER SET OCTETS")
		{ }

		protected override void ConfigureParameter(DbParameter parameter)
		{
			((FbParameter)parameter).FbDbType = FbDbType.Guid;
		}
	}
}
