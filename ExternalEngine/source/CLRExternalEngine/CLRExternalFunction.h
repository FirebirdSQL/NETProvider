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

#ifndef CLREXTERNALFUNCTION_H
#define CLREXTERNALFUNCTION_H

#include "fb_external_engine.h"
#include "ObjectInstance.h"
#include "RuntimeHostException.h"

using namespace Firebird::CLRRuntimeHost;

class ErrorObject;
class ExternalFunction;

class CLRExternalFunction : public ExternalFunction
{
public:
	CLRExternalFunction(const wstring name, Firebird::CLRRuntimeHost::ObjectInstance* objectInstance);
	~CLRExternalFunction();
	void _stdcall execute(ErrorObject* error, int n, PARAMDSC** d, PARAMDSC* returnValue);
	void _stdcall release(ErrorObject* error);
		
protected:
	void Release();

private:
	wstring name;
	ObjectInstance* objectInstance;
};

#endif