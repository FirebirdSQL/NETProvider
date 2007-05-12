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
#include "RuntimeHostException.h"
#include "ObjectInstance.h"
#include "ApplicationDomain.h"

namespace Firebird
{
	namespace CLRRuntimeHost
	{
		using namespace std;

		ObjectInstance::ObjectInstance(ApplicationDomain* applicationDomain, const wstring assemblyName, const wstring className)
		{
			HRESULT hr = applicationDomain->GetHandle()->CreateInstance(
						_bstr_t(assemblyName.data()),
						_bstr_t(className.data()),
						&this->objectHandle); 

			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to create the object instance");
			}
		}

		ObjectInstance::~ObjectInstance()
		{
			this->Release();
		}

		void ObjectInstance::Release()
		{
			if (this->objectHandle != NULL)
			{
				this->objectHandle->Release();
				this->objectHandle = NULL;
			}
		}

		VARIANT ObjectInstance::Execute(const wstring methodName)
		{
			DISPPARAMS inputParameters = { NULL, NULL, 0, 0 };

			return this->Execute(methodName, inputParameters);
		}

		VARIANT ObjectInstance::Execute(const wstring methodName, DISPPARAMS inputParameters)
		{
			if (this->objectHandle == NULL)
			{
				throw new RuntimeHostException("ObjectHandle is no longer valid");
			}

			VARIANT v;
			DISPID	dispid;
			LPOLESTR szMethodName = _bstr_t(methodName.data());
			VARIANT result;
			EXCEPINFO pExcepInfo;
			unsigned int puArgErr = 0;
			
			// Initialze the variants
			VariantInit(&v);
			VariantInit(&result);
					
			HRESULT hr = this->objectHandle->Unwrap(&v);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to retrieve method information");
			}

			// The .NET Component should expose IDispatch
			IDispatch* pdispatch = v.pdispVal;				

			// Retrieve the DISPID
			hr = pdispatch->GetIDsOfNames(
								IID_NULL,
								&szMethodName,
								1,
								LOCALE_SYSTEM_DEFAULT,
								&dispid);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to retrieve method information");
			}

			// Invoke the method on the IDispatch Interface
			hr = pdispatch->Invoke(
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
				throw new RuntimeHostException("Error on method execution");
			}

			return result;
		}
	}
}