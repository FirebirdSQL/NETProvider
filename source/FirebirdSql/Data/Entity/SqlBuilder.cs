/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
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
 *  Copyright (c) 2008-2010 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 */

#if ((NET_35 && ENTITY_FRAMEWORK) || (NET_40))

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Entity
{
    /// <summary>
    /// This class is like StringBuilder.  While traversing the tree for the first time, 
    /// we do not know all the strings that need to be appended e.g. things that need to be
    /// renamed, nested select statements etc.  So, we use a builder that can collect
    /// all kinds of sql fragments.
    /// </summary>
    internal sealed class SqlBuilder : ISqlFragment
    {
        #region · Fields ·

        private List<object> sqlFragments;

        #endregion

        #region · Properties ·

        private List<object> SqlFragments
        {
            get
            {
                if (null == sqlFragments)
                {
                    sqlFragments = new List<object>();
                }
                return sqlFragments;
            }
        }

        #endregion

        #region · Methods ·

        /// <summary>
        /// Add an object to the list - we do not verify that it is a proper sql fragment
        /// since this is an internal method.
        /// </summary>
        /// <param name="s"></param>
        public void Append(object s)
        {
            Debug.Assert(s != null);
            SqlFragments.Add(s);
        }

        /// <summary>
        /// This is to pretty print the SQL.  The writer <see cref="SqlWriter.Write"/>
        /// needs to know about new lines so that it can add the right amount of 
        /// indentation at the beginning of lines.
        /// </summary>
        public void AppendLine()
        {
            SqlFragments.Add(Environment.NewLine);
        }

        /// <summary>
        /// Whether the builder is empty.  This is used by the <see cref="SqlGenerator.Visit(ProjectExpression)"/>
        /// to determine whether a sql statement can be reused.
        /// </summary>
        public bool IsEmpty
        {
            get { return ((null == sqlFragments) || (0 == sqlFragments.Count)); }
        }

        #endregion

        #region · ISqlFragment Members ·

        /// <summary>
        /// We delegate the writing of the fragment to the appropriate type.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            if (null != sqlFragments)
            {
                foreach (object o in sqlFragments)
                {
                    string str = (o as string);
                    if (null != str)
                    {
                        writer.Write(str);
                    }
                    else
                    {
                        ISqlFragment sqlFragment = (o as ISqlFragment);
                        if (null != sqlFragment)
                        {
                            sqlFragment.WriteSql(writer, sqlGenerator);
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
#endif