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
using System.Collections;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

namespace FirebirdSql.VisualStudio.DataTools
{
    internal class FbDataObjectIdentifierConverter : AdoDotNetObjectIdentifierConverter
    {
        #region · Fields ·

        private string[] reservedWords = null;

        #endregion

        #region · Constructors ·

        public FbDataObjectIdentifierConverter(IVsDataConnection connection)
            : base(connection)
        {
        }

        #endregion

        #region · Protected Methods ·

        /// <summary>
        /// This implements correct parsing of a string identifier into parts.
        /// </summary>
        protected override string[] SplitIntoParts(string typeName, string identifier)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }

            // Find the type in the data object support model
            IVsDataObjectType           type                = null;
            IVsDataObjectSupportModel   objectSupportModel  = Site.GetService(
                typeof(IVsDataObjectSupportModel)) as IVsDataObjectSupportModel;

            Debug.Assert(objectSupportModel != null);
            
            if (objectSupportModel != null && objectSupportModel.Types.ContainsKey(typeName))
            {
                type = objectSupportModel.Types[typeName];
            }

            if (type == null)
            {
                throw new ArgumentException("Invalid type " + typeName + ".");
            }

            // Split the string around '.' except when in an identifier part
            string[] arrIdentifier = new string[type.Identifier.Count];

            if (identifier != null)
            {
                int arrIndex = 0;
                int startIndex = 0;
                int endIndex = 0;
                char quote = '\0';
                while (endIndex < identifier.Length)
                {
                    if (identifier[endIndex] == '"' && quote == '\0')
                    {
                        // We entered a quoted identifier part using '"'
                        quote = '"';
                    }
                    else if (identifier[endIndex] == '"' && quote == '"')
                    {
                        if (endIndex < identifier.Length - 1 &&
                            identifier[endIndex + 1] == '"')
                        {
                            // We encountered an embedded quote in a quoted
                            // identifier part; skip it
                            endIndex++;
                        }
                        else
                        {
                            // We left a quoted identifier part using '"'
                            quote = '\0';
                        }
                    }
                    else if (identifier[endIndex] == '.' && quote == '\0')
                    {
                        // We encountered a separator outside of a quoted
                        // identifier part
                        if (arrIndex == arrIdentifier.Length)
                        {
                            throw new FormatException();
                        }
                        arrIdentifier[arrIndex] = identifier.Substring(startIndex, endIndex - startIndex);
                        arrIndex++;
                        startIndex = endIndex + 1;
                    }
                    endIndex++;
                }

                if (identifier.Length > 0)
                {
                    if (arrIndex == arrIdentifier.Length)
                    {
                        throw new FormatException();
                    }
                    arrIdentifier[arrIndex] = identifier.Substring(startIndex);
                }
            }

            // Shift the elements in the array so they are right aligned
            int shiftCount = 0;
            for (int i = arrIdentifier.Length - 1; i >= 0; i--)
            {
                if (arrIdentifier[i] != null)
                {
                    break;
                }
                
                shiftCount++;
            }
            
            string[] tempArray = arrIdentifier;
            arrIdentifier = new string[tempArray.Length];
            Array.Copy(tempArray, 0, arrIdentifier, shiftCount, arrIdentifier.Length - shiftCount);

            return arrIdentifier;
        }

        /// <summary>
        /// This method removes quotes from an identifier part, and unescapes
        /// the string.
        /// </summary>
        protected override object UnformatPart(string typeName, string identifierPart)
        {
            if (identifierPart == null)
            {
                return null;
            }

            string part = identifierPart.Trim();
            
            if (part.StartsWith("\"", StringComparison.Ordinal))
            {
                if (!part.EndsWith("\"", StringComparison.Ordinal))
                {
                    throw new FormatException();
                }
            
                return part.Substring(1, part.Length - 2).Replace("\"\"", "\"");
            }

            return part;
        }

        /// <summary>
        /// SQL Server has strict rules defined for when an identifier must
        /// be quoted.  This method simulates the server behavior to ensure
        /// all identifiers are quoted correctly on the client side.  Note
        /// it is not entirely correct for all cases but is sufficient for
        /// sample purposes.
        /// </summary>
        protected override bool RequiresQuoting(string identifierPart)
        {
            // If string does not follow rules for regular (unquoted)
            // identifier, then it must be quoted.

            // 0) If empty string, does not need to be quoted
            if (identifierPart.Length == 0)
            {
                return false;
            }

            // 1) Cannot be a Transact-SQL reserved word (either upper or lower case)
            if (this.reservedWords == null)
            {
                IVsDataSourceInformation sourceInformation = Site.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;

                Debug.Assert(sourceInformation != null);
                
                if (sourceInformation != null)
                {
                    this.reservedWords = sourceInformation[DataSourceInformation.ReservedWords].ToString().Split(',');
                }
            }

            foreach (string reservedWord in this.reservedWords)
            {
                if (identifierPart.Equals(reservedWord, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This method adds quotes to an identifier part, if necessary, and
        /// escapes the quote character in the string.
        /// </summary>
        protected override string FormatPart(string typeName,object identifierPart, DataObjectIdentifierFormat format)
        {
            if (identifierPart == null || identifierPart is DBNull)
            {
                return null;
            }
            
            string strIdentifierPart = identifierPart.ToString();
            
            if ((format & DataObjectIdentifierFormat.WithQuotes) != 0 && this.RequiresQuoting(strIdentifierPart))
            {
                return ('"' + strIdentifierPart + '"');
            }
            return strIdentifierPart;
        }

        #endregion
    }
}
