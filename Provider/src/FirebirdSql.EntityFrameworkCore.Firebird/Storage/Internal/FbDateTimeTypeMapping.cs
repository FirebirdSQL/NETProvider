using System;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal
{
	public class FbDateTimeTypeMapping : DateTimeTypeMapping
	{
		readonly FbDbType _fbDbType;

		public FbDateTimeTypeMapping(string storeType, FbDbType fbDbType)
			: base(storeType)
		{
			_fbDbType = fbDbType;
		}

		protected override void ConfigureParameter(DbParameter parameter)
		{
			((FbParameter)parameter).FbDbType = _fbDbType;
		}

		protected override string SqlLiteralFormatString
		{
			get
			{
				switch (_fbDbType)
				{
					case FbDbType.TimeStamp:
						return "{0:yyyy-MM-dd HH:mm:ss}";
					case FbDbType.Date:
						return "{0:yyyy-MM-dd}";
					case FbDbType.Time:
						return "{0:HH:mm:ss}";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}
