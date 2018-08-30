/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net), Jean Ressouche, Rafael Almeida (ralms@ralms.net)

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
						throw new ArgumentOutOfRangeException(nameof(_fbDbType), $"{nameof(_fbDbType)}={_fbDbType}");
				}
			}
		}
	}
}
