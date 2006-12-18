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
    internal class FbDataObjectIdentifierConverter : AdoDotNetObjectIdentifierConverter
    {
        #region · Fields ·

        private DataConnection connection;

        #endregion

        #region · Constructors ·

        public FbDataObjectIdentifierConverter(DataConnection connection) 
            : base(connection)
        {
            this.connection = connection;
        }

        #endregion

        #region · Protected Methods ·

        protected override string FormatPart(string typeName, object identifierPart, bool withQuotes)
        {
            string openQuote    = (string)this.connection.SourceInformation[DataSourceInformation.IdentifierOpenQuote];
            string closeQuote   = (string)this.connection.SourceInformation[DataSourceInformation.IdentifierCloseQuote];
            string identifier   = (identifierPart is string) ? (string)identifierPart : null;

            if (withQuotes && identifier != null && !this.IsQuoted(identifier))
            {
                if (!identifier.StartsWith(openQuote))
                {
                    identifier = openQuote + identifier;
                }

                if (!identifier.EndsWith(closeQuote))
                {
                    identifier = identifier + openQuote;
                }
            }

            // return ((identifier != null) ? identifier : String.Empty);
            return identifier;
        }

        #endregion

        #region · Private Methods ·

        private bool IsQuoted(string value)
        {
            string openQuote    = (string)this.connection.SourceInformation[DataSourceInformation.IdentifierOpenQuote];
            string closeQuote   = (string)this.connection.SourceInformation[DataSourceInformation.IdentifierOpenQuote];

            return (value.StartsWith(openQuote) && value.EndsWith(closeQuote));
        }

        #endregion
    }
}
