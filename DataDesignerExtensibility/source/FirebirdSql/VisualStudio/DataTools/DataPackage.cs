/*
 *  Visual Studio DDEX Provider for Firebird
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
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace FirebirdSql.VisualStudio.DataTools
{
    [Guid(GuidList.GuidDataToolsPkgString)]
    [DefaultRegistryRoot(@"Microsoft\VisualStudio\9.0")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideService(typeof(FbDataProviderObjectFactory))]
    [ProvideMenuResource(1000, 1)]
    public sealed class DataPackage : Package
    {
        #region · Constructors ·

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public DataPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        #endregion
        
        #region · Package Members ·

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));

            IServiceContainer serviceContainer = this as IServiceContainer;

            if (serviceContainer != null)
            {
                serviceContainer.AddService(typeof(FbDataProviderObjectFactory), delegate { return new FbDataProviderObjectFactory(); }, true);
            }
        }

        #endregion
    }
}