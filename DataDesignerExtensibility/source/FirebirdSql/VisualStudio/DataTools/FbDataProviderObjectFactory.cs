/*
 *  Visual Studio 2005 DDEX Provider for Firebird
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

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;

namespace FirebirdSql.VisualStudio.DataTools
{
    [Guid(GuidList.GuidObjectFactoryServiceString)]
    internal class FbDataProviderObjectFactory : AdoDotNetProviderObjectFactory
    {
        #region · Constructors ·

        public FbDataProviderObjectFactory() : base()
        {
            System.Diagnostics.Trace.WriteLine("FbDataProviderObjectFactory()");
        }

        #endregion

        #region · Methods ·

        public override object CreateObject(Type objectType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("FbDataProviderObjectFactory::CreateObject({0})", objectType.FullName));

            if (objectType == typeof(DataConnectionSupport))
            {
                return new FbDataConnectionSupport();
            }
            else if (objectType == typeof(DataConnectionUIControl))
            {
                return new FbDataConnectionUIControl();
            }
            else if (objectType == typeof(DataConnectionProperties))
            {
                return new FbDataConnectionProperties();
            }

            return base.CreateObject(objectType);
        }

        #endregion
    }
}
