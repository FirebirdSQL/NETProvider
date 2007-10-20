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
 *  Copyright (c) 2007 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *  
 *  Based on the Microsoft Entity Framework Provider Sample Beta 1
 */

#if (NET_35 && ENTITY_FRAMEWORK)

using System;
using System.Globalization;

namespace FirebirdSql.Data.Entity
{
    /// <summary>
    /// FirstClause represents the FIRST expression in a SqlSelectStatement. 
    /// It has a count property, which indicates how many FIRST rows should be selected and a 
    /// boolen WithTies property.
    /// </summary>
    internal class FirstClause : ISqlFragment
    {
        #region · Fields ·

        private ISqlFragment firstCount;

        #endregion

        #region · Internal Properties ·

        /// <summary>
        /// How many top rows should be selected.
        /// </summary>
        internal ISqlFragment TopCount
        {
            get { return this.firstCount; }
        }

        #endregion

        #region · Constructors ·

        /// <summary>
        /// Creates a TopClause with the given topCount and withTies.
        /// </summary>
        /// <param name="topCount"></param>
        internal FirstClause(ISqlFragment topCount)
        {
            this.firstCount = topCount;
        }

        /// <summary>
        /// Creates a TopClause with the given topCount and withTies.
        /// </summary>
        /// <param name="topCount"></param>
        internal FirstClause(int topCount)
        {
            SqlBuilder sqlBuilder = new SqlBuilder();
            sqlBuilder.Append(topCount.ToString(CultureInfo.InvariantCulture));
            this.firstCount = sqlBuilder;
        }

        #endregion

        #region · ISqlFragment Members ·

        /// <summary>
        /// Write out the FIRST part of sql select statement 
        /// It basically writes FIRST (X).
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("TOP (");
            this.TopCount.WriteSql(writer, sqlGenerator);
            writer.Write(")");

            writer.Write(" ");
        }

        #endregion
    }
}

#endif