/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;

public class FbGuidTypeMapping : GuidTypeMapping
{
	public FbGuidTypeMapping()
		: base("CHAR(16) CHARACTER SET OCTETS")
	{ }

	protected FbGuidTypeMapping(RelationalTypeMappingParameters parameters)
		: base(parameters)
	{ }

	protected override void ConfigureParameter(DbParameter parameter)
	{
		((FbParameter)parameter).FbDbType = FbDbType.Guid;
	}

	protected override string GenerateNonNullSqlLiteral(object value)
	{
		return $"CHAR_TO_UUID('{value}')";
	}

	protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
		=> new FbGuidTypeMapping(parameters);
}
