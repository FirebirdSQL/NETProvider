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
		ValueTask<byte[]> ReadBytes(byte[] buffer, int count, AsyncWrappingCommonArgs async);
		ValueTask<byte[]> ReadOpaque(int length, AsyncWrappingCommonArgs async);
		ValueTask<byte[]> ReadBuffer(AsyncWrappingCommonArgs async);
		ValueTask<string> ReadString(AsyncWrappingCommonArgs async);
		ValueTask<string> ReadString(int length, AsyncWrappingCommonArgs async);
		ValueTask<string> ReadString(Charset charset, AsyncWrappingCommonArgs async);
		ValueTask<string> ReadString(Charset charset, int length, AsyncWrappingCommonArgs async);
		ValueTask<short> ReadInt16(AsyncWrappingCommonArgs async);
		ValueTask<int> ReadInt32(AsyncWrappingCommonArgs async);
		ValueTask<long> ReadInt64(AsyncWrappingCommonArgs async);
		ValueTask<Guid> ReadGuid(AsyncWrappingCommonArgs async);
		ValueTask<float> ReadSingle(AsyncWrappingCommonArgs async);
		ValueTask<double> ReadDouble(AsyncWrappingCommonArgs async);
		ValueTask<DateTime> ReadDateTime(AsyncWrappingCommonArgs async);
		ValueTask<DateTime> ReadDate(AsyncWrappingCommonArgs async);
		ValueTask<TimeSpan> ReadTime(AsyncWrappingCommonArgs async);
		ValueTask<decimal> ReadDecimal(int type, int scale, AsyncWrappingCommonArgs async);
		ValueTask<bool> ReadBoolean(AsyncWrappingCommonArgs async);
		ValueTask<FbZonedDateTime> ReadZonedDateTime(bool isExtended, AsyncWrappingCommonArgs async);
		ValueTask<FbZonedTime> ReadZonedTime(bool isExtended, AsyncWrappingCommonArgs async);
		ValueTask<FbDecFloat> ReadDec16(AsyncWrappingCommonArgs async);
		ValueTask<FbDecFloat> ReadDec34(AsyncWrappingCommonArgs async);
		ValueTask<BigInteger> ReadInt128(AsyncWrappingCommonArgs async);
		ValueTask<IscException> ReadStatusVector(AsyncWrappingCommonArgs async);
		ValueTask<int> ReadOperation(AsyncWrappingCommonArgs async);
	}
}
