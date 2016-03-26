/*
 *  Visual Studio DDEX Provider for FirebirdClient
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
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;

namespace FirebirdSql.VisualStudio.DataTools
{
    /// <summary>
    /// Provides information about an ADO.NET data source in the form of 
    /// properties passed as name/value pairs.
    /// </summary>
    internal class FbDataSourceInformation : AdoDotNetDataSourceInformation
    {
        #region · Constructors ·

        public FbDataSourceInformation(DataConnection connection)
            : base(connection)
        {
            base.AddProperty(AdoDotNetDataSourceInformation.CatalogSupported, false);
            base.AddProperty(AdoDotNetDataSourceInformation.CatalogSupportedInDml, false);
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultSchema);
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultCatalog, null);
            base.AddProperty(AdoDotNetDataSourceInformation.DefaultSchema, null);
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierOpenQuote, "\"");
            base.AddProperty(AdoDotNetDataSourceInformation.IdentifierCloseQuote, "\"");
            base.AddProperty(AdoDotNetDataSourceInformation.ParameterPrefix, "@");
            base.AddProperty(AdoDotNetDataSourceInformation.ParameterPrefixInName, true);
            base.AddProperty(AdoDotNetDataSourceInformation.ProcedureSupported, true);
            base.AddProperty(AdoDotNetDataSourceInformation.QuotedIdentifierPartsCaseSensitive, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SchemaSupported, false);
            base.AddProperty(AdoDotNetDataSourceInformation.SchemaSupportedInDml, false);
            base.AddProperty(AdoDotNetDataSourceInformation.ServerSeparator, ".");
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsAnsi92Sql, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsQuotedIdentifierParts, true);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsCommandTimeout, false);
            base.AddProperty(AdoDotNetDataSourceInformation.SupportsQuotedIdentifierParts, true);
            base.AddProperty("DesktopDataSource", true);
            base.AddProperty("LocalDatabase", true);
        }

        #endregion
    }
}
