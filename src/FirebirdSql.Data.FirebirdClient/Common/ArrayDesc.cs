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
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Common;

[StructLayout(LayoutKind.Auto)]
internal struct ArrayDesc
{
	public byte DataType { get; set; }
	public short Scale { get; set; }
	public short Length { get; set; }
	public string FieldName { get; set; }
	public string RelationName { get; set; }
	public short Dimensions { get; set; }
	// Specifies wheter array is to be accesed in
	// row mayor or column-mayor order
	public short Flags { get; set; }
	public ArrayBound[] Bounds { get; set; }
}
