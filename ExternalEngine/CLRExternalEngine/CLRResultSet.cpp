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
#include "CLRResultSet.h"
#include "Convert.h"

CLRResultSet::CLRResultSet(IUnknown* resultset)
{
	this->resultset = (IDispatch*)resultset;
}

CLRResultSet::~CLRResultSet()
{
	if (this->resultset != NULL)
	{
		this->Execute(NULL, L"Close");
		this->resultset = NULL;
	}
}

bool _stdcall CLRResultSet::fetch(ErrorObject* error)
{
	if (this->resultset == NULL)
	{
		return false;
	}

	VARIANT result;

	result = this->Execute(error, L"Read");

	if (result.vt != VT_BOOL)
	{
		error->addString("CLRResultSet::fetch ( Invalid return type, expected bool )", fb_string_ascii);

		return false;
	}

	return (result.boolVal == 0) ? false : true;
}

void _stdcall CLRResultSet::getValue(ErrorObject* error, int n, PARAMDSC* rv)
{
	if (this->resultset == NULL)
	{
		error->addString("Invalid resultset object", fb_string_ascii);
	}
	else
	{
		VARIANT result;

		DISPPARAMS inputParameters;

		VariantInit(&result);

		inputParameters.cArgs			= 1;
		inputParameters.cNamedArgs		= 0;
		inputParameters.rgvarg			= new VARIANT[1];
		inputParameters.rgvarg[0].vt	= VT_I4;
		inputParameters.rgvarg[0].lVal	= n;

		result = this->Execute(error, L"GetValue", inputParameters);

		Convert::ToParamDsc(result, rv);

		delete inputParameters.rgvarg;
	}
}

void _stdcall CLRResultSet::release(ErrorObject* error)
{
	this->Execute(error, L"Close");
	this->resultset = NULL;
}

VARIANT CLRResultSet::Execute(ErrorObject* error, const wstring methodName)
{
	DISPPARAMS inputParameters = { NULL, NULL, 0, 0 };

	return this->Execute(error, methodName, inputParameters);
}

VARIANT CLRResultSet::Execute(ErrorObject* error, const wstring methodName, DISPPARAMS inputParameters)
{
	VARIANT	result;

	if (this->resultset != NULL)
	{
		HRESULT		hr;
		DISPID		dispid;
		LPOLESTR	szMethodName = _bstr_t(methodName.data());
		EXCEPINFO	pExcepInfo;
		unsigned int puArgErr = 0;
		
		VariantInit(&result);

		hr = this->resultset->GetIDsOfNames(
						IID_NULL,
						&szMethodName,
						1,
						LOCALE_SYSTEM_DEFAULT,
						&dispid);

		if (!SUCCEEDED(hr))
		{
			if (error != NULL)
			{
				error->addString("CLRResultSet::Execute ( Error getting method information )", fb_string_ascii);
			}
		}
		else
		{
			// Invoke the method on the IDispatch Interface
			hr = this->resultset->Invoke(
							dispid,
							IID_NULL,
							LOCALE_SYSTEM_DEFAULT,
							DISPATCH_METHOD,
							&inputParameters,
							&result,
							&pExcepInfo,
							&puArgErr);

			if (!SUCCEEDED(hr))
			{
				if (error != NULL)
				{
					error->addString("CLRResultSet::Execute ( Error invoking method )", fb_string_ascii);
				}
			}
		}
	}

	return result;
}