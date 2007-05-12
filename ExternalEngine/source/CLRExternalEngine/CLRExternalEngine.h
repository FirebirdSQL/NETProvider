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

#ifndef CLREXTERNALENGINE_H
#define CLREXTERNALENGINE_H

#include "fb_external_engine.h"
#include "RuntimeHost.h"

using namespace Firebird::CLRRuntimeHost;

class ErrorObject;
class ExternalEngine;
class ExternalFunction;
class ExternalProcedure;

class CLRExternalEngine : public ExternalEngine
{
public:
	CLRExternalEngine();
	~CLRExternalEngine();
	ExternalFunction* _stdcall makeFunction(ErrorObject* error, const char* name, int n, const char* signature);
	ExternalProcedure* _stdcall makeProcedure(ErrorObject* error, const char* name, int n, const char* signature);

	void _stdcall attachThread();
	void _stdcall detachThread();

private:
	RuntimeHost* runtime;
	ApplicationDomain* domain;
};

#endif