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
		ValueTask<byte[]> ReadBytesAsync(byte[] buffer, int count, AsyncWrappingCommonArgs async);
		ValueTask<byte[]> ReadOpaqueAsync(int length, AsyncWrappingCommonArgs async);
		ValueTask<byte[]> ReadBufferAsync(AsyncWrappingCommonArgs async);
		ValueTask<string> ReadStringAsync(AsyncWrappingCommonArgs async);
		ValueTask<string> ReadStringAsync(int length, AsyncWrappingCommonArgs async);
		ValueTask<string> ReadStringAsync(Charset charset, AsyncWrappingCommonArgs async);
		ValueTask<string> ReadStringAsync(Charset charset, int length, AsyncWrappingCommonArgs async);
		ValueTask<short> ReadInt16Async(AsyncWrappingCommonArgs async);
		ValueTask<int> ReadInt32Async(AsyncWrappingCommonArgs async);
		ValueTask<long> ReadInt64Async(AsyncWrappingCommonArgs async);
		ValueTask<Guid> ReadGuidAsync(AsyncWrappingCommonArgs async);
		ValueTask<float> ReadSingleAsync(AsyncWrappingCommonArgs async);
		ValueTask<double> ReadDoubleAsync(AsyncWrappingCommonArgs async);
		ValueTask<DateTime> ReadDateTimeAsync(AsyncWrappingCommonArgs async);
		ValueTask<DateTime> ReadDateAsync(AsyncWrappingCommonArgs async);
		ValueTask<TimeSpan> ReadTimeAsync(AsyncWrappingCommonArgs async);
		ValueTask<decimal> ReadDecimalAsync(int type, int scale, AsyncWrappingCommonArgs async);
		ValueTask<bool> ReadBooleanAsync(AsyncWrappingCommonArgs async);
		ValueTask<FbZonedDateTime> ReadZonedDateTimeAsync(bool isExtended, AsyncWrappingCommonArgs async);
		ValueTask<FbZonedTime> ReadZonedTimeAsync(bool isExtended, AsyncWrappingCommonArgs async);
		ValueTask<FbDecFloat> ReadDec16Async(AsyncWrappingCommonArgs async);
		ValueTask<FbDecFloat> ReadDec34Async(AsyncWrappingCommonArgs async);
		ValueTask<BigInteger> ReadInt128Async(AsyncWrappingCommonArgs async);
		ValueTask<IscException> ReadStatusVectorAsync(AsyncWrappingCommonArgs async);
		ValueTask<int> ReadOperationAsync(AsyncWrappingCommonArgs async);
	}
}
