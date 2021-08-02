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
		ValueTask FlushAsync(AsyncWrappingCommonArgs async);
		ValueTask WriteBytesAsync(byte[] buffer, int count, AsyncWrappingCommonArgs async);
		ValueTask WriteOpaqueAsync(byte[] buffer, AsyncWrappingCommonArgs async);
		ValueTask WriteOpaqueAsync(byte[] buffer, int length, AsyncWrappingCommonArgs async);
		ValueTask WriteBufferAsync(byte[] buffer, AsyncWrappingCommonArgs async);
		ValueTask WriteBufferAsync(byte[] buffer, int length, AsyncWrappingCommonArgs async);
		ValueTask WriteBlobBufferAsync(byte[] buffer, AsyncWrappingCommonArgs async);
		ValueTask WriteTypedAsync(int type, byte[] buffer, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(string value, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(short value, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(int value, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(long value, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(float value, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(double value, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(decimal value, int type, int scale, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(bool value, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(DateTime value, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(Guid value, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(FbDecFloat value, int size, AsyncWrappingCommonArgs async);
		ValueTask WriteAsync(BigInteger value, AsyncWrappingCommonArgs async);
		ValueTask WriteDateAsync(DateTime value, AsyncWrappingCommonArgs async);
		ValueTask WriteTimeAsync(TimeSpan value, AsyncWrappingCommonArgs async);
	}
}
