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
#include "CLRExternalFunction.h"
#include "Convert.h"

using namespace std;
using namespace Firebird::CLRRuntimeHost;

CLRExternalFunction::CLRExternalFunction(const wstring name, ObjectInstance* objectInstance)
{
	this->name.append(name);
	this->objectInstance = objectInstance;
}

CLRExternalFunction::~CLRExternalFunction()
{
	this->Release();
}

void _stdcall CLRExternalFunction::execute(ErrorObject* error, int n, PARAMDSC** d, PARAMDSC* returnValue)
{
	if (this->objectInstance != NULL)
	{
		VARIANT result;

		VariantInit(&result);

		// Convert parameters
		DISPPARAMS parameters = Convert::ToDispParams(n, d);

		try
		{
			// Execute the method call
			result = this->objectInstance->Execute(name, parameters);

			if (returnValue != NULL)
			{
				Convert::ToParamDsc(result, returnValue);
			}

			if (parameters.rgvarg)
			{
				// Free resources that are no more needed
				delete parameters.rgvarg;
			}
		}
		catch (RuntimeHostException* e)
		{
			if (parameters.rgvarg)
			{
				// Free resources that are no more needed
				delete parameters.rgvarg;
			}

			error->addString(e->GetMessage().data(), fb_string_ascii);
		}
		catch (...)
		{
			error->addString("Unknown Error on procedure execution", fb_string_ascii);
		}
	}
}

void _stdcall CLRExternalFunction::release(ErrorObject* error)
{
	this->Release();
}

void CLRExternalFunction::Release()
{
	if (this->objectInstance != NULL)
	{
		this->objectInstance->Release();
		this->objectInstance = NULL;
	}
	this->name.clear();
}