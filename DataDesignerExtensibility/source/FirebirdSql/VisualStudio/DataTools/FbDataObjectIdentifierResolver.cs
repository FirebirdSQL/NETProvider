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

namespace FirebirdSql.VisualStudio.DataTools
{
    internal class FbDataObjectIdentifierResolver : DataObjectIdentifierResolver
    {
        #region · Private Fields ·

        private DataConnection connection;

        #endregion

        #region · Constructors ·

        public FbDataObjectIdentifierResolver(DataConnection connection) 
            : base()
        {
            System.Diagnostics.Trace.WriteLine("FbDataObjectIdentifierResolver()");
            this.connection = connection;
        }

        #endregion

        #region · Methods ·

        protected override object[] QuickContractIdentifier(string typeName, object[] fullIdentifier)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("FbDataObjectIdentifierResolver::QuickContractIdentifier({0},...)", typeName));

            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }

            if (typeName == FbDataObjectTypes.Root)
            {
                return base.QuickContractIdentifier(typeName, fullIdentifier);
            }

            object[] identifier = null;
            int length = this.GetIdentifierLength(typeName);
            if (length == -1)
            {
                throw new NotSupportedException();
            }
            identifier = new object[length];

            if (fullIdentifier != null)
            {
                fullIdentifier.CopyTo(identifier, length - fullIdentifier.Length);
            }

            if (identifier.Length > 0)
            {
                identifier[0] = null;
            }

            if (identifier.Length > 1)
            {
                identifier[1] = null;
            }

            return identifier;
        }

        protected override object[] QuickExpandIdentifier(string typeName, object[] partialIdentifier)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("FbDataObjectIdentifierResolver::QuickExpandIdentifier({0},...)", typeName));

            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }

            // Create an identifier array of the correct full length based on
            // the object type
            object[] identifier = null;
            int length = this.GetIdentifierLength(typeName);
            if (length == -1)
            {
                throw new NotSupportedException();
            }
            identifier = new object[length];

            // If the input identifier is not null, copy it to the full
            // identifier array.  If the input identifier's length is less
            // than the full length we assume the more specific parts are
            // specified and thus copy into the rightmost portion of the
            // full identifier array.
            if (partialIdentifier != null)
            {
                if (partialIdentifier.Length > length)
                {
                    throw new InvalidOperationException();
                }

                partialIdentifier.CopyTo(identifier, length - partialIdentifier.Length);
            }

            if (length > 0)
            {
                identifier[0] = null;
            }

            if (length > 1)
            {
                identifier[1] = null;
            }

            return identifier;
        }

        #endregion

        #region · Private Methods ·

        private int GetIdentifierLength(string typeName)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("GetIdentifierLength({0})", typeName));

            switch (typeName)
            {
                case FbDataObjectTypes.Root:
                    return 0;

                case FbDataObjectTypes.Table:
                case FbDataObjectTypes.View:
                case FbDataObjectTypes.StoredProcedure:
                    return 3;

                case FbDataObjectTypes.TableColumn:
                case FbDataObjectTypes.ViewColumn:
                case FbDataObjectTypes.StoredProcedureParameter:
                    return 4;
                               
                default:
                    return -1;
            }
        }

        #endregion
    }
}
