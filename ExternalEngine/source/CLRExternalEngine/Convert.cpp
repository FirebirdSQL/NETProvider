/*
 *  .NET External Procedure Engine for Firebird
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

#include "stdafx.h"
#include "Convert.h"
#include "RuntimeHostException.h"
#include "CLRExternalEngine.h"
#include <math.h>

using namespace Firebird::CLRRuntimeHost;

Convert::Convert()
{
}

void Convert::ToParamDsc(VARIANT source, PARAMDSC* target)
{
	switch (source.vt)
	{
	case VT_NULL:
		target->dsc_flags |= DSC_null;
		break;

	case VT_BSTR:
		{
		wstring s (source.bstrVal);

		int		start	= (target->dsc_dtype == dtype_varying) ? 2 : 0;
		short	length	= (target->dsc_dtype == dtype_varying) ? (short)s.size() : target->dsc_length;
		
		// If the string length is greater than the length defined in the parameter
		// return it as NULL ( look if we sould raise an exception here !!!! )
		if (length > (target->dsc_length - start))
		{
			target->dsc_flags |= DSC_null;
		}
		else
		{
			target->dsc_address = new unsigned char[length + start];

			// Fill the parameter with spaces
			for (int i = start; i < (length + start); i++)
			{
				target->dsc_address[i] = 32;
			}

			// Set string length for variying types
			if (target->dsc_dtype == dtype_varying)
			{
				target->dsc_length		= length;
				target->dsc_address[0]	= length >> 0;
				target->dsc_address[1]	= length >> 8;
			}

			// wcstomb(

			// Fill the paremeter with the real value
			for (int i = 0, j = start; i < (int)s.size(); i++, j++)
			{
				target->dsc_address[j] = (char)s[i];
			}
		}
		}
		break;

	case VT_I1:		// Byte
		target->dsc_dtype = dtype_byte;
		*((BYTE*)target->dsc_address) = source.bVal;
		break;

	case VT_I2:		// Int16
		target->dsc_dtype = dtype_short;
		*((SHORT*)target->dsc_address) = source.iVal;
		break;

	case VT_I4:		// Int32
		target->dsc_dtype = dtype_long;
		*((LONG*)target->dsc_address) = source.lVal;
		break;

	case VT_I8:		// Int64
		if (target->dsc_dtype != dtype_quad)
		{
			target->dsc_dtype = dtype_int64;
		}
		*((LONGLONG*)target->dsc_address) = source.llVal;
		break;

	case VT_R8:		// Double
		target->dsc_dtype = dtype_double;
		*((DOUBLE*)target->dsc_address) = source.dblVal;
		break;

	case VT_R4:		// Float
		target->dsc_dtype = dtype_real;
		*((FLOAT*)target->dsc_address) = source.fltVal;
		break;

	case VT_DECIMAL:	// Decimal
		target->dsc_dtype = dtype_int64;
		if (source.decVal.scale == 0)
		{
			*((LONGLONG*)target->dsc_address) = (LONGLONG)(source.decVal.Lo64 * pow((double)10, (-1) * target->dsc_scale));
		}
		else
		{
			target->dsc_scale = (-1) * source.decVal.scale;
			*((LONGLONG*)target->dsc_address) = (LONGLONG)source.decVal.Lo64;
		}
		break;

	case VT_DATE:
		BSTR timestamp;
		DATE inDateTime;
		UDATE outDateTime;
		
		switch (target->dsc_dtype)
		{
		case dtype_sql_date:
			VarBstrFromDate(source.date, 0, VAR_DATEVALUEONLY, &timestamp);

			VarDateFromStr(timestamp, 0, 0, &inDateTime);
			VarUdateFromDate(inDateTime, 0, &outDateTime);

			*((LONG*)target->dsc_address) = Convert::EncodeDate(outDateTime);
			break;

		case dtype_sql_time:
			VarBstrFromDate(source.date, 0, VAR_TIMEVALUEONLY, &timestamp);

			VarDateFromStr(timestamp, 0, 0, &inDateTime);
			VarUdateFromDate(inDateTime, 0, &outDateTime);

			*((LONG*)target->dsc_address) = Convert::EncodeTime(outDateTime);
			break;

		case dtype_timestamp:
			VarBstrFromDate(source.date, 0, 0, &timestamp);
			break;
		}
		break;
	}
}

DISPPARAMS Convert::ToDispParams(int count, PARAMDSC** source)
{
	if (count == 0)
	{
		DISPPARAMS parameters = { NULL, NULL, 0, 0 };

		return parameters;
	}

	DISPPARAMS target;

	target.cArgs		= count;
	target.cNamedArgs	= 0;
	target.rgvarg		= new VARIANT[count];

	for (int i = 0; i < count; i++)
	{
		PARAMDSC* parameter = source[i];

		switch (parameter->dsc_dtype)
		{
		case dtype_null:
			target.rgvarg[i].vt			= VT_NULL; 
			break;

		case dtype_cstring:		// string
		case dtype_text:
		case dtype_varying:	
			target.rgvarg[i].vt			= VT_BSTR;
			target.rgvarg[i].bstrVal	= _bstr_t((CHAR*)parameter->dsc_address);
			break;

		case dtype_byte:		// Byte
			target.rgvarg[i].vt			= VT_I1;
			target.rgvarg[i].bVal		= *((BYTE*)parameter->dsc_address);
			break;

		case dtype_short:		// Int16
			target.rgvarg[i].vt			= VT_I2;
			target.rgvarg[i].iVal		= *((SHORT*)parameter->dsc_address);
			break;

		case dtype_long:		// Int32
			target.rgvarg[i].vt			= VT_I4;
			target.rgvarg[i].lVal		= *((LONG*)parameter->dsc_address);
			break;

		case dtype_int64:		// Int64
			target.rgvarg[i].vt			= VT_I8;
			target.rgvarg[i].llVal		= *((LONGLONG*)parameter->dsc_address);
			break;

		case dtype_d_float:		// Double
		case dtype_double:		
			target.rgvarg[i].vt			= VT_R8;
			target.rgvarg[i].dblVal		= *((DOUBLE*)parameter->dsc_address);
			break;

		case dtype_real:		// Float
			target.rgvarg[i].vt			= VT_R4;
			target.rgvarg[i].fltVal		= *((FLOAT*)parameter->dsc_address);
			break;

		case dtype_sql_date:	// Date
			{
			BSTR strDate = Convert::DecodeDate(*((LONG*)parameter->dsc_address));
			DATE date;

			VarDateFromStr(_bstr_t(strDate), 0, VAR_DATEVALUEONLY, &date);

			target.rgvarg[i].vt		= VT_DATE;
			target.rgvarg[i].date	= date;
			}
			break;

		case dtype_sql_time:	// Time
			{
			BSTR strTime = Convert::DecodeTime(*((LONG*)parameter->dsc_address));
			DATE time;

			VarDateFromStr(_bstr_t(strTime), 0, VAR_TIMEVALUEONLY, &time);

			target.rgvarg[i].vt		= VT_DATE;
			target.rgvarg[i].date	= time;
			}
			break;

		case dtype_timestamp:	// Timestamp
			{
			LONGLONG longDate = *((LONGLONG*)parameter->dsc_address);
			BSTR strDate = Convert::DecodeDate(longDate);
			BSTR strTime = Convert::DecodeTime(longDate >> 32);
			DATE timestamp;	

			_bstr_t strTimestamp (strDate);

			strTimestamp += L" ";
			strTimestamp += strTime;
			
			VarDateFromStr(_bstr_t(strTimestamp), 0, 0, &timestamp);

			target.rgvarg[i].vt		= VT_DATE;
			target.rgvarg[i].date	= timestamp;
			}
			break;

		case dtype_blob:		// Blob
#pragma message ("Pending")
			break;		
		}
	}

	return target;
}

void Convert::Copy(PARAMDSC* source, PARAMDSC* target)
{
	if (source->dsc_dtype != target->dsc_dtype)
	{
		throw new RuntimeHostException("Invalid conversion");
	}

	target->dsc_dtype		= source->dsc_dtype;
	target->dsc_flags		= source->dsc_flags;
	target->dsc_length		= source->dsc_length;
	target->dsc_scale		= source->dsc_scale;
	target->dsc_sub_type	= source->dsc_sub_type;

	switch (source->dsc_dtype)
	{
	case dtype_varying:
#pragma message ("Pending")
		break;

	case dtype_cstring:
#pragma message ("Pending")
		break;

	case dtype_text:
#pragma message ("Pending")
		break;

	case dtype_byte:		// Byte
		*((BYTE*)target->dsc_address) = *((BYTE*)source->dsc_address);
		break;

	case dtype_short:		// Int16
		*((SHORT*)target->dsc_address) = *((SHORT*)source->dsc_address);
		break;

	case dtype_long:		// Int32
		*((LONG*)target->dsc_address) = *((LONG*)source->dsc_address);
		break;

	case dtype_int64:		// Int64
		*((LONGLONG*)target->dsc_address) = *((LONGLONG*)source->dsc_address);
		break;

	case dtype_double:		// Double
		*((DOUBLE*)target->dsc_address) = *((DOUBLE*)source->dsc_address);
		break;

	case dtype_real:		// Float
		*((FLOAT*)target->dsc_address) = *((FLOAT*)source->dsc_address);
		break;

		case dtype_sql_date:	// Date
#pragma message ("Pending")
			break;

		case dtype_sql_time:	// Time
#pragma message ("Pending")
			break;

		case dtype_timestamp:	// Timestamp
#pragma message ("Pending")
			break;

		case dtype_blob:		// Blob
#pragma message ("Pending")
			break;		
	}
}

int Convert::EncodeDate(UDATE date)
{
	int day, month, year;
	int c, ya;

	day		= date.st.wDay;
	month	= date.st.wMonth;
	year	= date.st.wYear;

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

BSTR Convert::DecodeDate(int date)
{
	int year, month, day, century;

	date	-= 1721119 - 2400001;
	century	= (4 * date - 1) / 146097;
	date	= 4 * date - 1 - 146097 * century;
	day		= date / 4;

	date = (4 * day + 3) / 1461;
	day = 4 * day + 3 - 1461 * date;
	day = (day + 4) / 4;

	month = (5 * day - 3) / 153;
	day = 5 * day - 3 - 153 * month;
	day = (day + 5) / 5;

	year = 100 * century + date;

	if (month < 10)
	{
		month += 3;
	}
	else
	{
		month -= 9;
		year += 1;
	}

	_bstr_t strDate (new char[200]);

	swprintf_s(strDate, 200, L"%i-%i-%i", year, month, day);

	return strDate.copy(true);
}

int Convert::EncodeTime(UDATE time)
{
	int millisInDay =
		(int)(time.st.wHour * 3600000 +
		time.st.wMinute * 60000 +
		time.st.wSecond * 1000 +
		time.st.wMilliseconds) * 10;

	return millisInDay;
}

BSTR Convert::DecodeTime(int time)
{
	int millisInDay = time / 10;
	int hour		= millisInDay / 3600000;
	int minute		= (millisInDay - hour * 3600000) / 60000;
	int second		= (millisInDay - hour * 3600000 - minute * 60000) / 1000;
	int millisecond = millisInDay - hour * 3600000 - minute * 60000 - second * 1000;

	_bstr_t strTime (new char[200]);

	swprintf_s(strTime, 200, L"%i:%i:%i", hour, minute, second);

	return strTime.copy(true);
}