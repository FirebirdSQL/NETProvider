/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.ibphoenix.com/main.nfs?a=ibphoenix&l=;PAGES;NAME='ibp_idpl'
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;

namespace FirebirdSql.Data.Firebird.Gds
{
	[StructLayout(LayoutKind.Explicit)]
	internal sealed class DoubleLayout
	{
		[FieldOffset(0)] public double d;
		[FieldOffset(0)] public int i0;
		[FieldOffset(4)] public int i4;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal sealed class FloatLayout
	{
		[FieldOffset(0)] public float f;
		[FieldOffset(0)] public int i0;
	}

	internal class GdsDecodeType
	{
		public static System.Decimal DecodeDecimal(object d, int scale, int sqltype)
		{
			long	divisor = 1;
			decimal returnValue;

			if (scale < 0)
			{
				int exp = scale * (-1);
				divisor = (long)System.Math.Pow(10, exp);
			}
			
			switch (sqltype & ~1)
			{
				case GdsCodes.SQL_SHORT:
				case GdsCodes.SQL_LONG:
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					returnValue = Convert.ToDecimal(d) / divisor;
					break;
											
				case GdsCodes.SQL_DOUBLE:
				default:
					returnValue = Convert.ToDecimal(d);
					break;
			}

			return returnValue;
		}

		public static System.DateTime DecodeTime(int sql_time) 
		{
			return new System.DateTime((sql_time / 10000) * 1000 * 10000L + 621355968000000000);
		}

		public static System.DateTime DecodeDate(int sql_date) 
		{
			int year, month, day, century;

			sql_date	-= 1721119 - 2400001;
			century		= (4 * sql_date - 1) / 146097;
			sql_date	= 4 * sql_date - 1 - 146097 * century;
			day			= sql_date / 4;

			sql_date	= (4 * day + 3) / 1461;
			day			= 4 * day + 3 - 1461 * sql_date;
			day			= (day + 4) / 4;

			month		= (5 * day - 3) / 153;
			day			= 5 * day - 3 - 153 * month;
			day			= (day + 5) / 5;

			year		= 100 * century + sql_date;

			if (month < 10) 
			{
				month += 3;
			} 
			else 
			{
				month	-= 9;
				year	+= 1;
			}

			DateTime date = new System.DateTime(year, month, day);		

			return date.Date;
		}
	}
}
