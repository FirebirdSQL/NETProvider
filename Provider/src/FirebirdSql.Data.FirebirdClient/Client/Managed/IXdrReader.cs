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
		Task<byte[]> ReadBytes(byte[] buffer, int count, AsyncWrappingCommonArgs async);
		Task<byte[]> ReadOpaque(int length, AsyncWrappingCommonArgs async);
		Task<byte[]> ReadBuffer(AsyncWrappingCommonArgs async);
		Task<string> ReadString(AsyncWrappingCommonArgs async);
		Task<string> ReadString(int length, AsyncWrappingCommonArgs async);
		Task<string> ReadString(Charset charset, AsyncWrappingCommonArgs async);
		Task<string> ReadString(Charset charset, int length, AsyncWrappingCommonArgs async);
		Task<short> ReadInt16(AsyncWrappingCommonArgs async);
		Task<int> ReadInt32(AsyncWrappingCommonArgs async);
		Task<long> ReadInt64(AsyncWrappingCommonArgs async);
		Task<Guid> ReadGuid(AsyncWrappingCommonArgs async);
		Task<float> ReadSingle(AsyncWrappingCommonArgs async);
		Task<double> ReadDouble(AsyncWrappingCommonArgs async);
		Task<DateTime> ReadDateTime(AsyncWrappingCommonArgs async);
		Task<DateTime> ReadDate(AsyncWrappingCommonArgs async);
		Task<TimeSpan> ReadTime(AsyncWrappingCommonArgs async);
		Task<decimal> ReadDecimal(int type, int scale, AsyncWrappingCommonArgs async);
		Task<bool> ReadBoolean(AsyncWrappingCommonArgs async);
		Task<FbZonedDateTime> ReadZonedDateTime(bool isExtended, AsyncWrappingCommonArgs async);
		Task<FbZonedTime> ReadZonedTime(bool isExtended, AsyncWrappingCommonArgs async);
		Task<FbDecFloat> ReadDec16(AsyncWrappingCommonArgs async);
		Task<FbDecFloat> ReadDec34(AsyncWrappingCommonArgs async);
		Task<BigInteger> ReadInt128(AsyncWrappingCommonArgs async);
		Task<IscException> ReadStatusVector(AsyncWrappingCommonArgs async);
		Task<int> ReadOperation(AsyncWrappingCommonArgs async);
	}
}
