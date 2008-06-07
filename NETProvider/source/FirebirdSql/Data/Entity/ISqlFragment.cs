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
 *  Copyright (c) 2008 Jiri Cincura (jiri@cincura.net)
 *  All Rights Reserved.
 *  
 *  Based on the Microsoft Entity Framework Provider Sample SP1 Beta 1
 */

#if (NET_35 && ENTITY_FRAMEWORK)

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
    /// Represents the sql fragment for any node in the query tree.
    /// </summary>
    /// <remarks>
    /// The nodes in a query tree produce various kinds of sql
    /// <list type="bullet">
    /// <item>A select statement.</item>
    /// <item>A reference to an extent. (symbol)</item>
    /// <item>A raw string.</item>
    /// </list>
    /// We have this interface to allow for a common return type for the methods
    /// in the expression visitor <see cref="ExpressionVisitor{T}"/>
    /// 
    /// At the end of translation, the sql fragments are converted into real strings.
    /// </remarks>
    internal interface ISqlFragment
    {
        /// <summary>
        /// Write the string represented by this fragment into the stream.
        /// </summary>
        /// <param name="writer">The stream that collects the strings.</param>
        /// <param name="sqlGenerator">Context information used for renaming.
        /// The global lists are used to generated new names without collisions.</param>
        void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator);
    }
}
#endif