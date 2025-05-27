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

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace FirebirdSql.EntityFrameworkCore.Firebird.Storage.Internal;

public class FbSqlGenerationHelper : RelationalSqlGenerationHelper, IFbSqlGenerationHelper
{
	public FbSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
		: base(dependencies)
	{ }

	public virtual string StringLiteralQueryType(string s, bool isUnicode = true, string storeTypeNameBase = "", int size = 0)
	{
		var maxSize = MinimumStringQueryTypeLength(s);
		string typeName;
		if (storeTypeNameBase.Equals("BLOB SUB_TYPE TEXT", StringComparison.OrdinalIgnoreCase))
		{
			typeName = "VARCHAR";
		}
		else
		{
			typeName = IsEmpty(storeTypeNameBase) ? "VARCHAR" : storeTypeNameBase;
		}

		var charset = isUnicode ? " CHARACTER SET UTF8" : string.Empty;
		return $"{typeName}({maxSize}){charset}";
	}

	public virtual string StringParameterQueryType(bool isUnicode, string storeTypeNameBase = "", int size = 0)
	{
		int maxSize;
		string typeName;
		if (storeTypeNameBase.Equals("BLOB SUB_TYPE TEXT", StringComparison.OrdinalIgnoreCase))
		{
			maxSize = (isUnicode ? FbTypeMappingSource.UnicodeVarcharMaxSize : FbTypeMappingSource.VarcharMaxSize);
			typeName = "VARCHAR";
		}
		else
		{
			maxSize = size > 0 ? size : (isUnicode ? FbTypeMappingSource.UnicodeVarcharMaxSize : FbTypeMappingSource.VarcharMaxSize);
			typeName = IsEmpty(storeTypeNameBase) ? "VARCHAR" : storeTypeNameBase;
		}
		return $"{typeName}({maxSize})";
	}

	public virtual void GenerateBlockParameterName(StringBuilder builder, string name)
	{
		builder.Append(":").Append(name);
	}

	public virtual string AlternativeStatementTerminator => "~";

	private int MinimumStringQueryTypeLength(string s)
	{
		var length = s?.Length ?? 0;
		if (length == 0)
			length = 1;
		return length;
	}

	private bool IsEmpty(string storeTypeNameBase)
	{
		return (storeTypeNameBase == null || string.IsNullOrEmpty(storeTypeNameBase) || string.IsNullOrWhiteSpace(storeTypeNameBase));
	}
}
