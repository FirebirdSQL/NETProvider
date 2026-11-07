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

//$Authors = F.D.Castel

using System;

namespace FirebirdSql.Data.FirebirdClient;

// CLR type of DECFLOAT values returned by FbDataReader.GetValue/GetFieldType, FbCommand.ExecuteScalar and output parameters.
[Serializable]
public enum FbDecFloatType
{
	// DECFLOAT(16) and DECFLOAT(34) values are FirebirdSql.Data.Types.FbDecFloat.
	FbDecFloat,
	// DECFLOAT(16) values are System.Numerics.Decimal64 and DECFLOAT(34) values are System.Numerics.Decimal128 (.NET 11 or later).
	Ieee754,
}
