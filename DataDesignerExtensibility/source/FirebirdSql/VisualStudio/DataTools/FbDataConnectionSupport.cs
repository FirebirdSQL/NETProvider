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
using System.Data;
using System.Diagnostics;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

namespace FirebirdSql.VisualStudio.DataTools
{
    internal class FbDataConnectionSupport : AdoDotNetConnectionSupport
    {
        #region · Constructors ·

        public FbDataConnectionSupport() 
        {
            System.Diagnostics.Trace.WriteLine("FbDataConnectionSupport()");
        }

        #endregion

        #region · Protected Methods ·

        protected override object CreateService(IServiceContainer container, Type serviceType)
        {
            if (serviceType == typeof(IDSRefBuilder))
            {
                return new DSRefBuilder(Site);
            }
            if (serviceType == typeof(IVsDataObjectIdentifierConverter))
            {
                return new FbDataObjectIdentifierConverter(Site);
            }
            if (serviceType == typeof(IVsDataObjectIdentifierResolver))
            {
                return new FbDataObjectIdentifierResolver(Site);
            }
            //if (serviceType == typeof(IVsDataObjectMemberComparer))
            //{
            //    return new FbDataObjectMemberComparer(Site);
            //}
            //if (serviceType == typeof(IVsDataObjectSelector))
            //{
            //    return new FbDataObjectSelector(Site);
            //}
            if (serviceType == typeof(IVsDataObjectSupport))
            {
                return new DataObjectSupport(GetType().Namespace + ".FbDataObjectSupport", GetType().Assembly);
            }
            if (serviceType == typeof(IVsDataSourceInformation))
            {
                return new FbDataSourceInformation(Site);
            }

            return base.CreateService(container, serviceType);
        }

        #endregion
    }
}
