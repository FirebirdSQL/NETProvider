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
#include "ApplicationDomain.h"
#include "RuntimeHost.h"
#include "RuntimeHostException.h"
#include "ObjectInstance.h"

namespace Firebird
{
	namespace CLRRuntimeHost
	{
		ApplicationDomain::ApplicationDomain(_AppDomain* domain, _Evidence* evidence, IAppDomainSetup* domainSetup)
		{
			this->domainSetup	= domainSetup;
			this->domainEvidence= evidence;
			this->domain		= domain;
		}

		ApplicationDomain::~ApplicationDomain()
		{
			this->Release();
		}

		void ApplicationDomain::Release()
		{
			if (this->domain != NULL)
			{
				this->domain->Release();
				this->domain = NULL;
			}
			if (this->domainSetup != NULL)
			{
				this->domainSetup->Release();
				this->domainSetup = NULL;
			}
			if (this->domainEvidence != NULL)
			{
				this->domainEvidence->Release();
				this->domainEvidence = NULL;
			}
		}

		ObjectInstance* ApplicationDomain::CreateInstance(const wstring assemblyName, const wstring className)
		{
			return new ObjectInstance(this, assemblyName, className);
		}

		_AppDomain* ApplicationDomain::GetHandle()
		{
			return this->domain;
		}
	}
}