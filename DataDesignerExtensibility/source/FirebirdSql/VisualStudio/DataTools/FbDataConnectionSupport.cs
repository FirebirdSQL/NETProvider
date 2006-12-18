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
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;

namespace FirebirdSql.VisualStudio.DataTools
{
    internal class FbDataConnectionSupport : AdoDotNetConnectionSupport
    {
        #region · Constructors ·

        public FbDataConnectionSupport() 
            : base("FirebirdSql.Data.FirebirdClient")
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionSupport()");
        }

        #endregion

        #region · Protected Methods ·

        protected override DataSourceInformation CreateDataSourceInformation()
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionSupport::CreateDataSourceInformation()");

            return new FbDataSourceInformation(base.Site as DataConnection);
        }

        protected override DataObjectIdentifierConverter CreateObjectIdentifierConverter()
        {
            return new FbDataObjectIdentifierConverter(base.Site as DataConnection);
        }

        protected override object GetServiceImpl(Type serviceType)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("FbDataConnectionSupport::GetServiceImpl({0})", serviceType.FullName));

            if (serviceType == typeof(DataViewSupport))
            {
                return new FbDataViewSupport();
            }
            else if (serviceType == typeof(DataObjectSupport))
            {
                return new FbDataObjectSupport();
            }
            else if (serviceType == typeof(DataObjectIdentifierResolver))
            {
                return new FbDataObjectIdentifierResolver(base.Site as DataConnection);
            }

            return base.GetServiceImpl(serviceType);
        }

        #endregion
    }
}
