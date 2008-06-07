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

namespace FirebirdSql.Data.Entity
{
    /// <summary>
    /// A SqlSelectStatement represents a canonical SQL SELECT statement.
    /// It has fields for the 5 main clauses
    /// <list type="number">
    /// <item>SELECT</item>
    /// <item>FROM</item>
    /// <item>WHERE</item>
    /// <item>GROUP BY</item>
    /// <item>ORDER BY</item>
    /// </list>
    /// We do not have HAVING, since it does not correspond to anything in the CommandTree.
    /// Each of the fields is a SqlBuilder, so we can keep appending SQL strings
    /// or other fragments to build up the clause.
    ///
    /// We have a IsDistinct property to indicate that we want distict columns.
    /// This is given out of band, since the input expression to the select clause
    /// may already have some columns projected out, and we use append-only SqlBuilders.
    /// The DISTINCT is inserted when we finally write the object into a string.
    /// 
    /// Also, we have a Top property, which is non-null if the number of results should
    /// be limited to certain number. It is given out of band for the same reasons as DISTINCT.
    ///
    /// The FromExtents contains the list of inputs in use for the select statement.
    /// There is usually just one element in this - Select statements for joins may
    /// temporarily have more than one.
    ///
    /// If the select statement is created by a Join node, we maintain a list of
    /// all the extents that have been flattened in the join in AllJoinExtents
    /// <example>
    /// in J(j1= J(a,b), c)
    /// FromExtents has 2 nodes JoinSymbol(name=j1, ...) and Symbol(name=c)
    /// AllJoinExtents has 3 nodes Symbol(name=a), Symbol(name=b), Symbol(name=c)
    /// </example>
    ///
    /// If any expression in the non-FROM clause refers to an extent in a higher scope,
    /// we add that extent to the OuterExtents list.  This list denotes the list
    /// of extent aliases that may collide with the aliases used in this select statement.
    /// It is set by <see cref="SqlGenerator.Visit(VariableReferenceExpression)"/>.
    /// An extent is an outer extent if it is not one of the FromExtents.
    /// </summary>
    /// <remarks
    /// ----------------------------------------
    /// Select statements and select expressions
    /// ----------------------------------------
    /// 
    /// Semantics:
    /// 
    /// A select statement is used to return data to the caller (PSQL module or the
    /// client program). Select expressions retrieve parts of data that construct both
    /// the final the result set or any of the intermediate sets. Select expressions
    /// are also known as subqueries.
    /// 
    /// Syntax rules:
    /// 
    /// <select statement> ::=
    /// <select expression> [FOR UPDATE] [WITH LOCK]
    /// 
    ///   <select expression> ::=
    ///     <query specification> [UNION [{ALL | DISTINCT}] <query specification>]
    /// 
    ///   <query specification> ::=
    ///     SELECT [FIRST <value>] [SKIP <value>] <select list>
    ///     FROM <table expression list>
    ///     WHERE <search condition>
    ///     GROUP BY <group value list>
    ///     HAVING <group condition>
    ///     PLAN <plan item list>
    ///     ORDER BY <sort value list>
    ///     ROWS <value> [TO <value>]
    /// 
    ///   <table expression> ::=
    ///     <table name> | <joined table> | <derived table>
    /// 
    ///   <joined table> ::=
    ///     {<cross join> | <qualified join>}
    /// 
    ///   <cross join> ::=
    ///     <table expression> CROSS JOIN <table expression>
    /// 
    ///   <qualified join> ::=
    ///     <table expression> [{INNER | {LEFT | RIGHT | FULL} [OUTER]}] JOIN <table expression>
    ///     ON <join condition>
    /// 
    ///   <derived table> ::=
    ///     '(' <select expression> ')'
    /// 
    /// Conclusions:
    /// 
    /// * FOR UPDATE mode and row locking can only be performed for a final dataset,
    /// they cannot be applied to a subquery
    /// * Unions are allowed inside any subquery
    /// * Clauses FIRST, SKIP, PLAN, ORDER BY, ROWS are allowed for any subquery
    /// 
    /// Notes:
    /// 
    /// * Either FIRST/SKIP or ROWS is allowed, they cannot be mixed together
    /// (a syntax error is thrown).
    /// 
    /// * INSERT statement accepts a select expression to create a dataset to be
    /// inserted into a table. So its SELECT part supports all the features defined
    /// above.
    /// 
    /// * UPDATE and DELETE statements are always based on an implicit cursor
    /// iterating through its target table and limited with the WHERE clause. You
    /// may also specify the final parts of the select expression syntax to limit
    /// the number of affected rows or optimize the statement. The following clauses
    /// are allowed at the end of the UPDATE/DELETE statements: PLAN, ORDER BY and
    /// ROWS.
    /// </remarks>
    internal sealed class SqlSelectStatement : ISqlFragment
    {
        #region � Fields �

        private bool                        isDistinct;
        private List<Symbol>                allJoinExtents;
        private List<Symbol>                fromExtents;
        private Dictionary<Symbol, bool>    outerExtents;
        private FirstClause                 first;
        private SkipClause                  skip;
        private SqlBuilder                  select = new SqlBuilder();
        private SqlBuilder                  from = new SqlBuilder();
        private SqlBuilder                  where;
        private SqlBuilder                  groupBy;
        private SqlBuilder                  orderBy;
        //indicates whether it is the top most select statement, 
        // if not Order By should be omitted unless there is a corresponding TOP
        private bool                        isTopMost;

        #endregion

        #region � Properties �

        public SqlBuilder OrderBy
        {
            get
            {
                if (null == orderBy)
                {
                    this.orderBy = new SqlBuilder();
                }
                return this.orderBy;
            }
        }

        #endregion

        #region � Internal Properties �

        /// <summary>
        /// Do we need to add a DISTINCT at the beginning of the SELECT
        /// </summary>
        internal bool IsDistinct
        {
            get { return this.isDistinct; }
            set { this.isDistinct = value; }
        }
        
        internal List<Symbol> AllJoinExtents
        {
            get { return this.allJoinExtents; }
            // We have a setter as well, even though this is a list,
            // since we use this field only in special cases.
            set { this.allJoinExtents = value; }
        }
        
        internal List<Symbol> FromExtents
        {
            get
            {
                if (null == this.fromExtents)
                {
                    this.fromExtents = new List<Symbol>();
                }
                return this.fromExtents;
            }
        }
        
        internal Dictionary<Symbol, bool> OuterExtents
        {
            get
            {
                if (null == outerExtents)
                {
                    this.outerExtents = new Dictionary<Symbol, bool>();
                }
                return outerExtents;
            }
        }

        internal FirstClause First
        {
            get { return this.first; }
            set
            {
                Debug.Assert(first == null, "SqlSelectStatement.Top has already been set");
                this.first = value;
            }
        }

        internal SkipClause Skip
        {
            get { return this.skip; }
            set
            {
                Debug.Assert(skip == null, "SqlSelectStatement.Skip has already been set");
                this.skip = value;
            }
        }

        internal SqlBuilder Select
        {
            get { return this.select; }
        }

        internal SqlBuilder From
        {
            get { return this.from; }
        }

        internal SqlBuilder Where
        {
            get
            {
                if (null == this.where)
                {
                    this.where = new SqlBuilder();
                }
                return this.where;
            }
        }

        internal SqlBuilder GroupBy
        {
            get
            {
                if (null == this.groupBy)
                {
                    this.groupBy = new SqlBuilder();
                }
                return this.groupBy;
            }
        }

        internal bool IsTopMost
        {
            get { return this.isTopMost; }
            set { this.isTopMost = value; }
        }

        #endregion

        #region � ISqlFragment Members �

        /// <summary>
        /// Write out a SQL select statement as a string.
        /// We have to
        /// <list type="number">
        /// <item>Check whether the aliases extents we use in this statement have
        /// to be renamed.
        /// We first create a list of all the aliases used by the outer extents.
        /// For each of the FromExtents( or AllJoinExtents if it is non-null),
        /// rename it if it collides with the previous list.
        /// </item>
        /// <item>Write each of the clauses (if it exists) as a string</item>
        /// </list>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="sqlGenerator"></param>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            #region Check if FROM aliases need to be renamed

            // Create a list of the aliases used by the outer extents
            // JoinSymbols have to be treated specially.
            List<string> outerExtentAliases = null;

            if ((null != outerExtents) && (0 < outerExtents.Count))
            {
                foreach (Symbol outerExtent in outerExtents.Keys)
                {
                    JoinSymbol joinSymbol = outerExtent as JoinSymbol;
                    if (joinSymbol != null)
                    {
                        foreach (Symbol symbol in joinSymbol.FlattenedExtentList)
                        {
                            if (null == outerExtentAliases)
                            {
                                outerExtentAliases = new List<string>();
                            }
                            outerExtentAliases.Add(symbol.NewName);
                        }
                    }
                    else
                    {
                        if (null == outerExtentAliases)
                        {
                            outerExtentAliases = new List<string>();
                        }
                        outerExtentAliases.Add(outerExtent.NewName);
                    }
                }
            }        

            // An then rename each of the FromExtents we have
            // If AllJoinExtents is non-null - it has precedence.
            // The new name is derived from the old name - we append an increasing int.
            List<Symbol> extentList = this.AllJoinExtents ?? this.fromExtents;
            if (null != extentList)
            {
                foreach (Symbol fromAlias in extentList)
                {
                    if ((null != outerExtentAliases) && outerExtentAliases.Contains(fromAlias.Name))
                    {
                        int i = sqlGenerator.AllExtentNames[fromAlias.Name];
                        string newName;

                        do
                        {
                            ++i;
                            newName = fromAlias.Name + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                        while (sqlGenerator.AllExtentNames.ContainsKey(newName));

                        sqlGenerator.AllExtentNames[fromAlias.Name] = i;
                        fromAlias.NewName = newName;

                        // Add extent to list of known names (although i is always incrementing, "prefix11" can
                        // eventually collide with "prefix1" when it is extended)
                        sqlGenerator.AllExtentNames[newName] = 0;
                    }

                    // Add the current alias to the list, so that the extents
                    // that follow do not collide with me.
                    if (null == outerExtentAliases)
                    {
                        outerExtentAliases = new List<string>();
                    }
                    outerExtentAliases.Add(fromAlias.NewName);
                }
            }

            #endregion

            // Increase the indent, so that the Sql statement is nested by one tab.
            writer.Indent += 1; // ++ can be confusing in this context

            writer.Write("SELECT ");
            if (this.IsDistinct)
            {
                writer.Write("DISTINCT ");
            }

            if (this.First != null)
            {
                this.First.WriteSql(writer, sqlGenerator);
            }

            if (this.Skip != null)
            {
                this.Skip.WriteSql(writer, sqlGenerator);
            }

            if ((this.select == null) || this.Select.IsEmpty)
            {
                Debug.Assert(false);  // we have removed all possibilities of SELECT *.
                writer.Write("*");
            }
            else
            {
                this.Select.WriteSql(writer, sqlGenerator);
            }

            writer.WriteLine();
            writer.Write("FROM ");
            this.From.WriteSql(writer, sqlGenerator);

            if ((this.where != null) && !this.Where.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("WHERE ");
                this.Where.WriteSql(writer, sqlGenerator);
            }

            if ((this.groupBy != null) && !this.GroupBy.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("GROUP BY ");
                this.GroupBy.WriteSql(writer, sqlGenerator);
            }

            if ((this.orderBy != null) && !this.OrderBy.IsEmpty && (this.IsTopMost || this.First != null))
            {
                writer.WriteLine();
                writer.Write("ORDER BY ");
                this.OrderBy.WriteSql(writer, sqlGenerator);
            }

            --writer.Indent;
        }

        #endregion
    }
}
#endif