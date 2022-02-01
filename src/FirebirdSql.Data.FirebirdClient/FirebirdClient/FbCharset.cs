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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;

namespace FirebirdSql.Data.FirebirdClient;

[Serializable]
public enum FbCharset
{
	Default = -1,
	None = 0,
	Octets = 1,
	Ascii = 2,
	UnicodeFss = 3,
	Utf8 = 4,
	ShiftJis0208 = 5,
	EucJapanese0208 = 6,
	Iso2022Japanese = 7,
	Dos437 = 10,
	Dos850 = 11,
	Dos865 = 12,
	Dos860 = 13,
	Dos863 = 14,
	Iso8859_1 = 21,
	Iso8859_2 = 22,
	Ksc5601 = 44,
	Dos861 = 47,
	Windows1250 = 51,
	Windows1251 = 52,
	Windows1252 = 53,
	Windows1253 = 54,
	Windows1254 = 55,
	Big5 = 56,
	Gb2312 = 57,
	Windows1255 = 58,
	Windows1256 = 59,
	Windows1257 = 60,
	// UTF-16
	// Utf16           = 61,
	// UTF-32
	// Utf32           = 62,
	// Russian KOI8R
	Koi8R = 63,
	// Ukrainian KOI8U
	Koi8U = 64,
	// TIS-620 Thai character set, single byte (since Firebird 2.1)
	TIS620 = 65,
}
