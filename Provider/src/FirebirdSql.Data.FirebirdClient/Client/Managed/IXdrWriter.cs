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
	interface IXdrWriter
	{
		ValueTask Flush(AsyncWrappingCommonArgs async);
		ValueTask WriteBytes(byte[] buffer, int count, AsyncWrappingCommonArgs async);
		ValueTask WriteOpaque(byte[] buffer, AsyncWrappingCommonArgs async);
		ValueTask WriteOpaque(byte[] buffer, int length, AsyncWrappingCommonArgs async);
		ValueTask WriteBuffer(byte[] buffer, AsyncWrappingCommonArgs async);
		ValueTask WriteBuffer(byte[] buffer, int length, AsyncWrappingCommonArgs async);
		ValueTask WriteBlobBuffer(byte[] buffer, AsyncWrappingCommonArgs async);
		ValueTask WriteTyped(int type, byte[] buffer, AsyncWrappingCommonArgs async);
		ValueTask Write(string value, AsyncWrappingCommonArgs async);
		ValueTask Write(short value, AsyncWrappingCommonArgs async);
		ValueTask Write(int value, AsyncWrappingCommonArgs async);
		ValueTask Write(long value, AsyncWrappingCommonArgs async);
		ValueTask Write(float value, AsyncWrappingCommonArgs async);
		ValueTask Write(double value, AsyncWrappingCommonArgs async);
		ValueTask Write(decimal value, int type, int scale, AsyncWrappingCommonArgs async);
		ValueTask Write(bool value, AsyncWrappingCommonArgs async);
		ValueTask Write(DateTime value, AsyncWrappingCommonArgs async);
		ValueTask Write(Guid value, AsyncWrappingCommonArgs async);
		ValueTask Write(FbDecFloat value, int size, AsyncWrappingCommonArgs async);
		ValueTask Write(BigInteger value, AsyncWrappingCommonArgs async);
		ValueTask WriteDate(DateTime value, AsyncWrappingCommonArgs async);
		ValueTask WriteTime(TimeSpan value, AsyncWrappingCommonArgs async);
	}
}
