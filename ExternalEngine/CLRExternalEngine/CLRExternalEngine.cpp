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
#include "fb_external_engine.h"
#include "ApplicationDomain.h"
#include "RuntimeHostException.h"
#include "CLRExternalFunction.h"
#include "CLRExternalProcedure.h"
#include "CLRExternalEngine.h"

using namespace Firebird::CLRRuntimeHost;

CLRExternalEngine::CLRExternalEngine()
{
	// Load the CLR Runtime
	this->runtime = new RuntimeHost();
	this->runtime->Load();
	this->runtime->Start();

	// Determines the Application directory
	wstring tempString (MAX_PATH, ' ');
	BSTR basePath (_bstr_t(tempString.data()));
	
	this->runtime->GetDefaultDomain()->GetHandle()->get_BaseDirectory(&basePath);

	// Create and configure a custom Application Domain
	IAppDomainSetup* domainSetup = this->runtime->CreateDomainSetup();

	domainSetup->put_ApplicationName(L"CLRExternalEngine");
	domainSetup->put_ApplicationBase(basePath);
	domainSetup->put_PrivateBinPath(_bstr_t(L"assembly"));
	domainSetup->put_CachePath(basePath);
	domainSetup->put_ShadowCopyFiles(_bstr_t(L"false"));
	domainSetup->put_ShadowCopyDirectories(basePath);

	this->domain = this->runtime->CreateDomain(L"CLRExternalEngine", NULL, domainSetup);
}

CLRExternalEngine::~CLRExternalEngine()
{
	if (this->runtime != NULL && this->runtime->IsStarted())
	{
		if (this->domain != NULL)
		{
			this->runtime->UnloadDomain(this->domain);
		}
		this->runtime->Unload();
		this->runtime	= NULL;
		this->domain	= NULL;
	}
}

ExternalFunction* _stdcall CLRExternalEngine::makeFunction(ErrorObject* error, const char* name, int n, const char* signature)
{
	if (this->runtime != NULL && this->runtime->IsLoaded() && this->runtime->IsStarted())
	{
		try
		{
			// Grab from the name the assembly name, the class name and the method name
			// format: className,assemblyName::methodName		

			std::string functionName (name);
			std::wstring qualifiedName (_bstr_t(functionName.c_str()));

			size_t index1 = qualifiedName.find(L",", 0);	
			size_t index2 = qualifiedName.find(L"::", 0);

			std::wstring className		= qualifiedName.substr(0, index1);
			std::wstring assemblyName	= qualifiedName.substr(index1 + 1, index2 - index1 - 1);
			std::wstring methodName		= qualifiedName.substr(index2 + 2);		

			return new CLRExternalFunction(methodName, this->domain->CreateInstance(assemblyName, className));
		}
		catch (RuntimeHostException* e)
		{
			error->addString(e->GetMessage().c_str(), fb_string_ascii);
		}
	}
	else
	{
		error->addString("There are no valid CLR runtime available", fb_string_ascii);
	}

	return NULL;
}

ExternalProcedure* _stdcall CLRExternalEngine::makeProcedure(ErrorObject* error, const char* name, int n, const char* signature)
{
	if (this->runtime != NULL && this->runtime->IsLoaded() && this->runtime->IsStarted())
	{
		try
		{
			// Grab from the name the assembly name, the class name and the method name
			// format: className,assemblyName::methodName		

			std::string functionName (name);
			std::wstring qualifiedName (_bstr_t(functionName.c_str()));

			size_t index1 = qualifiedName.find(L",", 0);	
			size_t index2 = qualifiedName.find(L"::", 0);

			std::wstring className		= qualifiedName.substr(0, index1);
			std::wstring assemblyName	= qualifiedName.substr(index1 + 1, index2 - index1 - 1);
			std::wstring methodName		= qualifiedName.substr(index2 + 2);		

			return new CLRExternalProcedure(methodName, this->domain->CreateInstance(assemblyName, className));
		}
		catch (RuntimeHostException* e)
		{
			error->addString(e->GetMessage().c_str(), fb_string_ascii);
		}
	}
	else
	{
		error->addString("There are no valid CLR runtime available", fb_string_ascii);
	}

	return NULL;
}

void _stdcall CLRExternalEngine::attachThread()
{
}

void _stdcall CLRExternalEngine::detachThread()
{
	if (this->runtime != NULL && this->runtime->IsStarted())
	{
		if (this->domain != NULL)
		{
			this->runtime->UnloadDomain(this->domain);
		}
		this->runtime->Unload();
		this->runtime	= NULL;
		this->domain	= NULL;
	}
}