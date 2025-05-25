﻿/*
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

using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;

public class FbTimeOnlyTypeMapping : TimeOnlyTypeMapping
{
	public FbTimeOnlyTypeMapping(string storeType)
		: base(storeType)
	{ }

	protected FbTimeOnlyTypeMapping(RelationalTypeMappingParameters parameters)
		: base(parameters)
	{ }

	protected override string GenerateNonNullSqlLiteral(object value)
	{
		return $"CAST('{value:HH\\:mm\\:ss.ffff}' AS TIME)";
	}

	protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
		=> new FbTimeOnlyTypeMapping(parameters);
}
