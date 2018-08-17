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

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Globalization;
using System.Net;

namespace FirebirdSql.Data.Common
{
	internal static class TypeEncoder
	{
		public static object EncodeDecimal(decimal d, int scale, int sqltype)
		{
			long multiplier = 1;

			if (scale < 0)
			{
				multiplier = (long)Math.Pow(10, -scale);
			}

			switch (sqltype & ~1)
			{
				case IscCodes.SQL_SHORT:
					return (short)(d * multiplier);

				case IscCodes.SQL_LONG:
					return (int)(d * multiplier);

				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					return (long)(d * multiplier);

				case IscCodes.SQL_DOUBLE:
				default:
					return d;
			}
		}

		public static int EncodeTime(TimeSpan t)
		{
			return (int)(t.Ticks / 1000L);
		}

		public static int EncodeDate(DateTime d)
		{
			int day, month, year;
			int c, ya;

			var calendar = new GregorianCalendar();

			day = calendar.GetDayOfMonth(d);
			month = calendar.GetMonth(d);
			year = calendar.GetYear(d);

			if (month > 2)
			{
				month -= 3;
			}
			else
			{
				month += 9;
				year -= 1;
			}

			c = year / 100;
			ya = year - 100 * c;

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
	}
}
