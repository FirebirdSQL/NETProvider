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
#include "RuntimeHost.h"
#include "RuntimeHostException.h"
#include "ApplicationDomain.h"

namespace Firebird
{
	namespace CLRRuntimeHost
	{
		RuntimeHost::RuntimeHost()
		{
			this->runtime	= NULL;
			this->isLoaded	= FALSE;
			this->isStarted	= FALSE;
		}

		RuntimeHost::~RuntimeHost()
		{
			this->Unload();
		}

		void RuntimeHost::Load()
		{
			this->Load(L"v2.0.50727");
		}

		void RuntimeHost::Load(const WCHAR* runtimeVersion)
		{
			if (this->isLoaded)
			{
				throw new RuntimeHostException("The CLR is already loaded");
			}

			HRESULT hr = CorBindToRuntimeEx(
							runtimeVersion,
							L"wks",
							STARTUP_LOADER_OPTIMIZATION_SINGLE_DOMAIN, // domain-neutral"ness"
							// | STARTUP_CONCURRENT_GC,  and gc settings - see below.
							CLSID_CorRuntimeHost, 
							IID_ICorRuntimeHost, 
							(void **)&this->runtime);

			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to Load the .NET CLR");
			}

			this->isLoaded = TRUE;
		}

		void RuntimeHost::Unload()
		{
			if (this->IsStarted() || this->IsLoaded())
			{
				this->Stop();
				this->runtime->Release();
				this->isLoaded = FALSE;
			}
		}

		void RuntimeHost::Start()
		{
			if (this->IsStarted())
			{
				throw new RuntimeHostException("The CLR should be loaded first");
			}

			HRESULT hr = this->runtime->Start();
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Cannot the start the CLR");
			}

			this->isStarted = true;
		}

		void RuntimeHost::Stop()
		{
			if (!this->IsStarted())
			{
				throw new RuntimeHostException("The CLR is already loaded and started");
			}

			HRESULT hr = this->runtime->Stop();
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Cannot stop the CLR");
			}

			this->isStarted = FALSE;
		}

		IAppDomainSetup* RuntimeHost::CreateDomainSetup()
		{
			IUnknown *domainSetupPunk		= NULL;
			IAppDomainSetup* domainSetup	= NULL;

			//
			// Domain Setup
			// 
			HRESULT hr = this->runtime->CreateDomainSetup(&domainSetupPunk);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to create the ApplicationDomain setup");
			}

			hr = domainSetupPunk->QueryInterface(__uuidof(IAppDomainSetup), (void**) &domainSetup);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to create the ApplicationDomain evidence");
			}

			return domainSetup;
		}

		_Evidence* RuntimeHost::CreateDomainEvidence()
		{
			IUnknown *domainEvidencePunk	= NULL;
			_Evidence* domainEvidence		= NULL;

			//
			// Domain Evidence ( Identity )
			//
			HRESULT hr = this->runtime->CreateEvidence(&domainEvidencePunk);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to create the ApplicationDomain evidence");
			}

			hr = domainEvidencePunk->QueryInterface(__uuidof(_Evidence), (void**) &domainEvidence);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to create the ApplicationDomain evidence");
			}

			return domainEvidence;
		}

		ApplicationDomain* RuntimeHost::GetDefaultDomain()
		{
			IUnknown* domainPunk = NULL;
			_AppDomain* domain = NULL;

			HRESULT hr = this->runtime->GetDefaultDomain(&domainPunk);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to create the ApplicationDomain evidence");
			}

			hr = domainPunk->QueryInterface(__uuidof(_AppDomain), (void**) &domain);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to create the ApplicationDomain evidence");
			}

			return new ApplicationDomain(domain, NULL, NULL);
		}

		ApplicationDomain* RuntimeHost::CreateDomain(const wstring domainName)
		{
			return this->CreateDomain(domainName, NULL, NULL);
		}

		ApplicationDomain* RuntimeHost::CreateDomain(const wstring domainName, _Evidence* domainEvidence)
		{
			return this->CreateDomain(domainName, domainEvidence, NULL);
		}

		ApplicationDomain* RuntimeHost::CreateDomain(const wstring domainName, _Evidence* domainEvidence, IAppDomainSetup* domainSetup)
		{
			if (!this->IsStarted())
			{
				throw new RuntimeHostException("The CLR is already loaded and started");
			}

			IUnknown *domainPunk	= NULL;
			_AppDomain *domain		= NULL;

			// Application Domain
			HRESULT hr = this->runtime->CreateDomainEx(domainName.data(), domainSetup, domainEvidence, &domainPunk);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to create the ApplicationDomain");
			}

			hr = domainPunk->QueryInterface(__uuidof(_AppDomain), (void**) &domain);
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Unable to create the ApplicationDomain");
			}

			return new ApplicationDomain(domain, domainEvidence, domainSetup);
		}

		void RuntimeHost::UnloadDomain(ApplicationDomain* domain)
		{
			if (!this->IsStarted())
			{
				throw new RuntimeHostException("The CLR is already loaded and started");
			}
			if (domain == NULL)
			{
				throw new RuntimeHostException("domain should be a valid ApplicationDomain");
			}

			HRESULT hr = this->runtime->UnloadDomain(domain->GetHandle());
			if (!SUCCEEDED(hr))
			{
				throw new RuntimeHostException("Cannot unload the requested application domain");
			}
			domain->Release();
		}

		ICorRuntimeHost* RuntimeHost::GetHandle()
		{
			return this->runtime;
		}

		bool RuntimeHost::IsLoaded()
		{
			return this->isLoaded;
		}

		bool RuntimeHost::IsStarted()
		{
			return this->isStarted;
		}
	}
}