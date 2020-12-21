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
		Task Flush(AsyncWrappingCommonArgs async);
		Task WriteBytes(byte[] buffer, int count, AsyncWrappingCommonArgs async);
		Task WriteOpaque(byte[] buffer, AsyncWrappingCommonArgs async);
		Task WriteOpaque(byte[] buffer, int length, AsyncWrappingCommonArgs async);
		Task WriteBuffer(byte[] buffer, AsyncWrappingCommonArgs async);
		Task WriteBuffer(byte[] buffer, int length, AsyncWrappingCommonArgs async);
		Task WriteBlobBuffer(byte[] buffer, AsyncWrappingCommonArgs async);
		Task WriteTyped(int type, byte[] buffer, AsyncWrappingCommonArgs async);
		Task Write(string value, AsyncWrappingCommonArgs async);
		Task Write(short value, AsyncWrappingCommonArgs async);
		Task Write(int value, AsyncWrappingCommonArgs async);
		Task Write(long value, AsyncWrappingCommonArgs async);
		Task Write(float value, AsyncWrappingCommonArgs async);
		Task Write(double value, AsyncWrappingCommonArgs async);
		Task Write(decimal value, int type, int scale, AsyncWrappingCommonArgs async);
		Task Write(bool value, AsyncWrappingCommonArgs async);
		Task Write(DateTime value, AsyncWrappingCommonArgs async);
		Task Write(Guid value, AsyncWrappingCommonArgs async);
		Task Write(FbDecFloat value, int size, AsyncWrappingCommonArgs async);
		Task Write(BigInteger value, AsyncWrappingCommonArgs async);
		Task WriteDate(DateTime value, AsyncWrappingCommonArgs async);
		Task WriteTime(TimeSpan value, AsyncWrappingCommonArgs async);
	}
}
