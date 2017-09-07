using System.Data.Common;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal
{
	public class FbStringTypeMapping : StringTypeMapping
	{
		readonly FbDbType _fbDbType;

		public FbStringTypeMapping(string storeType, FbDbType fbDbType, int? size = null)
			: base(storeType, unicode: true, size: size)
		{
			_fbDbType = fbDbType;
		}

		protected override void ConfigureParameter(DbParameter parameter)
		{
			((FbParameter)parameter).FbDbType = _fbDbType;
		}

		protected override string GenerateNonNullSqlLiteral(object value)
		{
#warning Duplicate from SqlGenerator.FormatBinary
			return string.Format("x'{0}'", ((byte[])value).ToHexString());
		}
	}
}
