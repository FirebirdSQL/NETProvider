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

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Diagnostics;
using System.Numerics;

namespace FirebirdSql.Data.Common;

internal static class Int128Helper
{
	public static BigInteger GetInt128(byte[] value)
	{
		Debug.Assert(value.Length == 16);
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(value);
		}
		return new BigInteger(value);
	}

	public static byte[] GetBytes(BigInteger value)
	{
		var result = value.ToByteArray();
		if (result.Length > 16)
		{
			throw new ArgumentOutOfRangeException("Value too big for Int128.");
		}
		if (result.Length < 16)
		{
			var padding = value.Sign == -1 ? (byte)255 : (byte)0;
			var tmp = new byte[16] { padding, padding, padding, padding, padding, padding, padding, padding, padding, padding, padding, padding, padding, padding, padding, padding };
			Buffer.BlockCopy(result, 0, tmp, 0, result.Length);
			result = tmp;
		}
		if (!BitConverter.IsLittleEndian)
		{
			Array.Reverse(result);
		}
		return result;
	}
}
