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
using System.Globalization;
using System.Net;
using System.Numerics;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Common;

internal static class TypeEncoder
{
	public static object EncodeDecimal(decimal d, int scale, int type)
	{
		var shift = scale < 0 ? -scale : scale;

		switch (type & ~1)
		{
			case IscCodes.SQL_SHORT:
				return (short)DecimalShiftHelper.ShiftDecimalRight(d, shift);

			case IscCodes.SQL_LONG:
				return (int)DecimalShiftHelper.ShiftDecimalRight(d, shift);

			case IscCodes.SQL_QUAD:
			case IscCodes.SQL_INT64:
				return (long)DecimalShiftHelper.ShiftDecimalRight(d, shift);

			case IscCodes.SQL_DOUBLE:
			case IscCodes.SQL_D_FLOAT:
				return (double)d;

			case IscCodes.SQL_INT128:
				return (BigInteger)DecimalShiftHelper.ShiftDecimalRight(d, shift);

			default:
				throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(type)}={type}");
		}
	}

	public static int EncodeTime(TimeSpan t)
	{
		return (int)(t.Ticks / 1000L);
	}
#if NET6_0_OR_GREATER
	public static int EncodeTime(TimeOnly t)
	{
		return (int)(t.Ticks / 1000L);
	}
#endif

	public static int EncodeDate(DateTime d)
	{
		var calendar = new GregorianCalendar();
		var day = calendar.GetDayOfMonth(d);
		var month = calendar.GetMonth(d);
		var year = calendar.GetYear(d);
		return EncodeDateImpl(year, month, day);
	}
#if NET6_0_OR_GREATER
	public static int EncodeDate(DateOnly d)
	{
		return EncodeDateImpl(d.Year, d.Month, d.Day);
	}
#endif
	static int EncodeDateImpl(int year, int month, int day)
	{
		if (month > 2)
		{
			month -= 3;
		}
		else
		{
			month += 9;
			year -= 1;
		}

		var c = year / 100;
		var ya = year - 100 * c;

		return ((146097 * c) / 4 + (1461 * ya) / 4 + (153 * month + 2) / 5 + day + 1721119 - 2400001);
	}

	public static byte[] EncodeBoolean(bool value)
	{
		return new[] { (byte)(value ? 1 : 0) };
	}

	public static byte[] EncodeGuid(Guid value)
	{
		var data = value.ToByteArray();
		var a = BitConverter.GetBytes(IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0)));
		var b = BitConverter.GetBytes(IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 4)));
		var c = BitConverter.GetBytes(IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 6)));
		return new[]
		{
			a[0], a[1], a[2], a[3],
			b[0], b[1],
			c[0], c[1],
			data[8], data[9], data[10], data[11], data[12], data[13], data[14], data[15]
		};
	}

	public static byte[] EncodeInt32(int value)
	{
		return BitConverter.GetBytes(IPAddress.NetworkToHostOrder(value));
	}

	public static byte[] EncodeInt64(long value)
	{
		return BitConverter.GetBytes(IPAddress.NetworkToHostOrder(value));
	}

	public static byte[] EncodeDec16(FbDecFloat value)
	{
		var result = DecimalCodec.DecFloat16.EncodeDecimal(value);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(result);
		}
		return result;
	}

	public static byte[] EncodeDec34(FbDecFloat value)
	{
		var result = DecimalCodec.DecFloat34.EncodeDecimal(value);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(result);
		}
		return result;
	}

	public static byte[] EncodeInt128(BigInteger value)
	{
		var result = Int128Helper.GetBytes(value);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(result);
		}
		return result;
	}
}
