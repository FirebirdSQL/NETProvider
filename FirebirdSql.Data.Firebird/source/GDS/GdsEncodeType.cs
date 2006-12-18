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
using System.Globalization;

namespace FirebirdSql.Data.Firebird.Gds
{
	internal class GdsEncodeType
	{
		public static object EncodeDecimal(System.Decimal d, int scale, int sqltype)
		{
			long multiplier = 1;
			
			if (scale < 0)
			{
				int exp = scale * (-1);
				multiplier = (long)System.Math.Pow(10, exp);
			}
			
			switch (sqltype & ~1)
			{
				case GdsCodes.SQL_SHORT:
					return (short)(d * multiplier);
				
				case GdsCodes.SQL_LONG:
					return (int)(d * multiplier);
				
				case GdsCodes.SQL_QUAD:
				case GdsCodes.SQL_INT64:
					return (long)(d * multiplier);
											
				case GdsCodes.SQL_DOUBLE:
				default:
				{
					return d;
				}
			}
		}

		public static int EncodeTime(System.DateTime d) 
		{
			GregorianCalendar calendar = new GregorianCalendar();			

			long millisInDay = calendar.GetHour(d) * 60 * 60 * 1000	+
				calendar.GetMinute(d) * 60 * 1000	+
				calendar.GetSecond(d) * 1000;				
			
			int iTime = (int) (millisInDay * 10);

			return iTime;
		}

		public static int EncodeDate(System.DateTime d)
		{			
			int day, month, year;
			int c, ya;

			GregorianCalendar calendar = new GregorianCalendar();

			day		= calendar.GetDayOfMonth(d);
			month	= calendar.GetMonth(d);
			year	= calendar.GetYear(d);

			if (month > 2) 
			{
				month -= 3;
			} 
			else
			{
				month	+= 9;
				year	-= 1;
			}

			c	= year / 100;
			ya	= year - 100 * c;

			return ((146097 * c) / 4	+
				(1461 * ya) / 4			+
				(153 * month + 2) / 5	+
				day + 1721119 - 2400001);
		}
	}
}
