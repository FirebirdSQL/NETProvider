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
using System.Data.Common;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;

namespace FirebirdSql.VisualStudio.DataTools
{
    internal class FbDataObjectEnumerator : AdoDotNetObjectEnumerator
    {
        #region · Constructors ·

        public FbDataObjectEnumerator() 
            : base()
        {
            System.Diagnostics.Trace.WriteLine("FbDataObjectEnumerator()");
        }

        #endregion

        #region · Methods ·

        public override DataReader EnumerateObjects(string typeName, object[] items, object[] restrictions, string sort, object[] parameters)
        {
            System.Diagnostics.Trace.WriteLine(String.Format("FbDataObjectEnumerator::EnumerateObjects({0})", typeName));

            DbConnection conn = Connection.GetLockedProviderObject() as DbConnection;

            try
            {
                if (typeName.Equals(FbDataObjectTypes.Root, StringComparison.InvariantCultureIgnoreCase))
                {
                    DataTable rootSchema = new DataTable();
                    rootSchema.Locale = System.Globalization.CultureInfo.CurrentCulture;

                    rootSchema.Columns.Add("Server", typeof(string));
                    rootSchema.Columns.Add("Database", typeof(string));

                    DataRow row = rootSchema.NewRow();

                    row["Server"]   = conn.DataSource;
                    row["Database"] = System.IO.Path.GetFileNameWithoutExtension(conn.Database);

                    rootSchema.Rows.Add(row);

                    return new AdoDotNetDataTableReader(rootSchema);
                }

                return base.EnumerateObjects(typeName, items, restrictions, sort, parameters);
            }
            finally
            {
                Connection.UnlockProviderObject();
            }
        }

        #endregion
    }
}
