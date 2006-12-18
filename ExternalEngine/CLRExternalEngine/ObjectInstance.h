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

#ifndef OBJECTINSTANCE_H
#define OBJECTINSTANCE_H

using namespace std;
using namespace mscorlib;

namespace Firebird
{
	namespace CLRRuntimeHost
	{
		class ApplicationDomain;

		class ObjectInstance
		{
		public:
			ObjectInstance(ApplicationDomain* applicationDomain, const wstring assemblyName, const wstring className);
			~ObjectInstance();
			VARIANT Execute(const wstring methodName);
			VARIANT Execute(const wstring methodName, DISPPARAMS inputParameters);
			void Release();	
			
		private:
			ApplicationDomain* applicationDomain;
			_ObjectHandle* objectHandle;
		};
	}
}


#endif