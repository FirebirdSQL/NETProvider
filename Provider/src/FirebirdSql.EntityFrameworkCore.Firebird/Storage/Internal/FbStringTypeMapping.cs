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

using System.Data.Common;
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
			return IsUnicode
				? $"_UTF8'{EscapeSqlLiteral((string)value)}'"
				: $"'{EscapeSqlLiteral((string)value)}'";
		}
	}
}
