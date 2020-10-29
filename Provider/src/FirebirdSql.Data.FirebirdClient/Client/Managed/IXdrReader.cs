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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Numerics;
using System.Threading.Tasks;
using FirebirdSql.Data.Common;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Client.Managed
{
	interface IXdrReader
	{
		byte[] ReadBytes(byte[] buffer, int count);
		Task<byte[]> ReadBytesAsync(byte[] buffer, int count);
		byte[] ReadOpaque(int length);
		byte[] ReadBuffer();
		string ReadString();
		string ReadString(int length);
		string ReadString(Charset charset);
		string ReadString(Charset charset, int length);
		short ReadInt16();
		int ReadInt32();
		Task<int> ReadInt32Async();
		long ReadInt64();
		Guid ReadGuid();
		float ReadSingle();
		double ReadDouble();
		DateTime ReadDateTime();
		DateTime ReadDate();
		TimeSpan ReadTime();
		decimal ReadDecimal(int type, int scale);
		bool ReadBoolean();
		FbZonedDateTime ReadZonedDateTime(bool isExtended);
		FbZonedTime ReadZonedTime(bool isExtended);
		FbDecFloat ReadDec16();
		FbDecFloat ReadDec34();
		BigInteger ReadInt128();
		IscException ReadStatusVector();
		int ReadOperation();
		Task<int> ReadOperationAsync();
	}
}
