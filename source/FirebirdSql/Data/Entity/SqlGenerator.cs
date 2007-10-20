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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

namespace FirebirdSql.Data.Entity
{
    /// <summary>
    /// Translates the command object into a SQL string that can be executed on
    /// Firebird SQL.
    /// </summary>
    /// <remarks>
    /// The translation is implemented as a visitor <see cref="ExpressionVisitor{T}"/>
    /// over the query tree.  It makes a single pass over the tree, collecting the sql
    /// fragments for the various nodes in the tree <see cref="ISqlFragment"/>.
    ///
    /// The major operations are
    /// <list type="bullet">
    /// <item>Select statement minimization.  Multiple nodes in the query tree
    /// that can be part of a single SQL select statement are merged. e.g. a
    /// Filter node that is the input of a Project node can typically share the
    /// same SQL statement.</item>
    /// <item>Alpha-renaming.  As a result of the statement minimization above, there
    /// could be name collisions when using correlated subqueries
    /// <example>
    /// <code>
    /// Filter(
    ///     b = Project( c.x
    ///         c = Extent(foo)
    ///         )
    ///     exists (
    ///         Filter(
    ///             c = Extent(foo)
    ///             b.x = c.x
    ///             )
    ///     )
    /// )
    /// </code>
    /// The first Filter, Project and Extent will share the same SQL select statement.
    /// The alias for the Project i.e. b, will be replaced with c.
    /// If the alias c for the Filter within the exists clause is not renamed,
    /// we will get <c>c.x = c.x</c>, which is incorrect.
    /// Instead, the alias c within the second filter should be renamed to c1, to give
    /// <c>c.x = c1.x</c> i.e. b is renamed to c, and c is renamed to c1.
    /// </example>
    /// </item>
    /// <item>Join flattening.  In the query tree, a list of join nodes is typically
    /// represented as a tree of Join nodes, each with 2 children. e.g.
    /// <example>
    /// <code>
    /// a = Join(InnerJoin
    ///     b = Join(CrossJoin
    ///         c = Extent(foo)
    ///         d = Extent(foo)
    ///         )
    ///     e = Extent(foo)
    ///     on b.c.x = e.x
    ///     )
    /// </code>
    /// If translated directly, this will be translated to
    /// <code>
    /// FROM ( SELECT c.*, d.*
    ///         FROM foo as c
    ///         CROSS JOIN foo as d) as b
    /// INNER JOIN foo as e on b.x' = e.x
    /// </code>
    /// It would be better to translate this as
    /// <code>
    /// FROM foo as c
    /// CROSS JOIN foo as d
    /// INNER JOIN foo as e on c.x = e.x
    /// </code>
    /// This allows the optimizer to choose an appropriate join ordering for evaluation.
    /// </example>
    /// </item>
    /// <item>Select * and column renaming.  In the example above, we noticed that
    /// in some cases we add <c>SELECT * FROM ...</c> to complete the SQL
    /// statement. i.e. there is no explicit PROJECT list.
    /// In this case, we enumerate all the columns available in the FROM clause
    /// This is particularly problematic in the case of Join trees, since the columns
    /// from the extents joined might have the same name - this is illegal.  To solve
    /// this problem, we will have to rename columns if they are part of a SELECT *
    /// for a JOIN node - we do not need renaming in any other situation.
    /// <see cref="SqlGenerator.AddDefaultColumns"/>.
    /// </item>
    /// </list>
    ///
    /// <para>
    /// Renaming issues.
    /// When rows or columns are renamed, we produce names that are unique globally
    /// with respect to the query.  The names are derived from the original names,
    /// with an integer as a suffix. e.g. CustomerId will be renamed to CustomerId1,
    /// CustomerId2 etc.
    ///
    /// Since the names generated are globally unique, they will not conflict when the
    /// columns of a JOIN SELECT statement are joined with another JOIN. 
    ///
    /// </para>
    ///
    /// <para>
    /// Record flattening.
    /// SQL server does not have the concept of records.  However, a join statement
    /// produces records.  We have to flatten the record accesses into a simple
    /// <c>alias.column</c> form.  <see cref="SqlGenerator.Visit(PropertyExpression)"/>
    /// </para>
    ///
    /// <para>
    /// Building the SQL.
    /// There are 2 phases
    /// <list type="numbered">
    /// <item>Traverse the tree, producing a sql builder <see cref="SqlBuilder"/></item>
    /// <item>Write the SqlBuilder into a string, renaming the aliases and columns
    /// as needed.</item>
    /// </list>
    ///
    /// In the first phase, we traverse the tree.  We cannot generate the SQL string
    /// right away, since
    /// <list type="bullet">
    /// <item>The WHERE clause has to be visited before the from clause.</item>
    /// <item>extent aliases and column aliases need to be renamed.  To minimize
    /// renaming collisions, all the names used must be known, before any renaming
    /// choice is made.</item>
    /// </list>
    /// To defer the renaming choices, we use symbols <see cref="Symbol"/>.  These
    /// are renamed in the second phase.
    ///
    /// Since visitor methods cannot transfer information to child nodes through
    /// parameters, we use some global stacks,
    /// <list type="bullet">
    /// <item>A stack for the current SQL select statement.  This is needed by
    /// <see cref="SqlGenerator.Visit(VariableReferenceExpression)"/> to create a
    /// list of free variables used by a select statement.  This is needed for
    /// alias renaming.
    /// </item>
    /// <item>A stack for the join context.  When visiting a <see cref="ScanExpression"/>,
    /// we need to know whether we are inside a join or not.  If we are inside
    /// a join, we do not create a new SELECT statement.</item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// Global state.
    /// To enable renaming, we maintain
    /// <list type="bullet">
    /// <item>The set of all extent aliases used.</item>
    /// <item>The set of all column aliases used.</item>
    /// </list>
    ///
    /// Finally, we have a symbol table to lookup variable references.  All references
    /// to the same extent have the same symbol.
    /// </para>
    ///
    /// <para>
    /// Sql select statement sharing.
    ///
    /// Each of the relational operator nodes
    /// <list type="bullet">
    /// <item>Project</item>
    /// <item>Filter</item>
    /// <item>GroupBy</item>
    /// <item>Sort/OrderBy</item>
    /// </list>
    /// can add its non-input (e.g. project, predicate, sort order etc.) to
    /// the SQL statement for the input, or create a new SQL statement.
    /// If it chooses to reuse the input's SQL statement, we play the following
    /// symbol table trick to accomplish renaming.  The symbol table entry for
    /// the alias of the current node points to the symbol for the input in
    /// the input's SQL statement.
    /// <example>
    /// <code>
    /// Project(b.x
    ///     b = Filter(
    ///         c = Extent(foo)
    ///         c.x = 5)
    ///     )
    /// </code>
    /// The Extent node creates a new SqlSelectStatement.  This is added to the
    /// symbol table by the Filter as {c, Symbol(c)}.  Thus, <c>c.x</c> is resolved to
    /// <c>Symbol(c).x</c>.
    /// Looking at the project node, we add {b, Symbol(c)} to the symbol table if the
    /// SQL statement is reused, and {b, Symbol(b)}, if there is no reuse.
    ///
    /// Thus, <c>b.x</c> is resolved to <c>Symbol(c).x</c> if there is reuse, and to
    /// <c>Symbol(b).x</c> if there is no reuse.
    /// </example>
    /// </para>
    /// </remarks>
    internal sealed class SqlGenerator : DbExpressionVisitor<ISqlFragment>
    {
        #region � Delegates �

        private delegate ISqlFragment FunctionHandler(SqlGenerator sqlgen, DbFunctionExpression functionExpr);

        #endregion

        #region � Static Members �

        #region � Fields �

        private static readonly Dictionary<string, FunctionHandler> FunctionHandlers = InitializeFunctionHandlers();
        private static readonly Dictionary<string, object> DatepartKeywords = InitializeDatepartKeywords();
        private static readonly char[] HexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        #endregion        

        #region � Initialization Methods �

        private static Dictionary<string, FunctionHandler> InitializeFunctionHandlers()
        {
            Dictionary<string, FunctionHandler> functionHandlers = new Dictionary<string, FunctionHandler>(5);

            functionHandlers.Add("concat", HandleConcatFunction);
            functionHandlers.Add("dateadd", HandleDatepartDateFunction);
            functionHandlers.Add("datediff", HandleDatepartDateFunction);
            functionHandlers.Add("datename", HandleDatepartDateFunction);
            functionHandlers.Add("datepart", HandleDatepartDateFunction);

            return functionHandlers;
        }

        private static Dictionary<string, object> InitializeDatepartKeywords()
        {
            //
            // valid datepart values
            //
            Dictionary<string, object> datepartKeywords = new Dictionary<string, object>(30);

            datepartKeywords.Add("d", null);
            datepartKeywords.Add("day", null);
            datepartKeywords.Add("dayofyear", null);
            datepartKeywords.Add("dd", null);
            datepartKeywords.Add("dw", null);
            datepartKeywords.Add("dy", null);
            datepartKeywords.Add("hh", null);
            datepartKeywords.Add("hour", null);
            datepartKeywords.Add("m", null);
            datepartKeywords.Add("mi", null);
            datepartKeywords.Add("millisecond", null);
            datepartKeywords.Add("minute", null);
            datepartKeywords.Add("mm", null);
            datepartKeywords.Add("month", null);
            datepartKeywords.Add("ms", null);
            datepartKeywords.Add("n", null);
            datepartKeywords.Add("q", null);
            datepartKeywords.Add("qq", null);
            datepartKeywords.Add("quarter", null);
            datepartKeywords.Add("s", null);
            datepartKeywords.Add("second", null);
            datepartKeywords.Add("ss", null);
            datepartKeywords.Add("week", null);
            datepartKeywords.Add("weekday", null);
            datepartKeywords.Add("wk", null);
            datepartKeywords.Add("ww", null);
            datepartKeywords.Add("y", null);
            datepartKeywords.Add("year", null);
            datepartKeywords.Add("yy", null);
            datepartKeywords.Add("yyyy", null);

            return datepartKeywords;
        }

        #endregion

        #endregion

        #region � Visitor parameter stacks �

        #region � Fields �

        /// <summary>
        /// Every relational node has to pass its SELECT statement to its children
        /// This allows them (VariableReferenceExpression eventually) to update the list of
        /// outer extents (free variables) used by this select statement.
        /// </summary>
        private Stack<SqlSelectStatement> selectStatementStack;

        /// <summary>
        /// Nested joins and extents need to know whether they should create
        /// a new Select statement, or reuse the parent's.  This flag
        /// indicates whether the parent is a join or not.
        /// </summary>
        private Stack<bool> isParentAJoinStack;

        #endregion

        #region � Properties �

        /// <summary>
        /// The top of the stack
        /// </summary>
        private SqlSelectStatement CurrentSelectStatement
        {
            // There is always something on the stack, so we can always Peek.
            get { return selectStatementStack.Peek(); }
        }

        /// <summary>
        /// The top of the stack
        /// </summary>
        private bool IsParentAJoin
        {
            // There might be no entry on the stack if a Join node has never
            // been seen, so we return false in that case.
            get { return isParentAJoinStack.Count == 0 ? false : isParentAJoinStack.Peek(); }
        }

        #endregion

        #endregion

        #region � Global lists and state �

        #region � Fields �

        private List<DbParameter> parameters;
        private Dictionary<string, int> allExtentNames;
        // For each column name, we store the last integer suffix that
        // was added to produce a unique column name.  This speeds up
        // the creation of the next unique name for this column name.
        private Dictionary<string, int> allColumnNames;
        private SymbolTable symbolTable = new SymbolTable();

        /// <summary>
        /// VariableReferenceExpressions are allowed only as children of PropertyExpression
        /// or MethodExpression.  The cheapest way to ensure this is to set the following
        /// property in VariableReferenceExpression and reset it in the allowed parent expressions.
        /// </summary>
        private bool isVarRefSingle = false;

        #endregion

        #region � Properties �

        internal Dictionary<string, int> AllExtentNames
        {
            get { return this.allExtentNames; }
        }

        internal Dictionary<string, int> AllColumnNames
        {
            get { return this.allColumnNames; }
        }

        internal List<DbParameter> Parameters
        {
            get { return this.parameters; }
        }

        #endregion

        #endregion

        #region � Constructors �

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlGenerator"/> class.
        /// </summary>
        private SqlGenerator()
        {
        }

        #endregion

        #region � Entry points �

        /// <summary>
        /// General purpose static function that can be called from System.Data assembly
        /// </summary>
        /// <param name="sqlVersion">Server version</param>
        /// <param name="tree">command tree</param>
        /// <param name="parameters">Parameters to add to the command tree corresponding
        /// to constants in the command tree. Used only in ModificationCommandTrees.</param>
        /// <returns>The string representing the SQL to be executed.</returns>
        internal static string GenerateSql(DbCommandTree tree, out List<DbParameter> parameters)
        {
            // Parameters initialization
            parameters = null;

            // Handle Query
            DbQueryCommandTree dbQueryCommandTree = tree as DbQueryCommandTree;

            if (dbQueryCommandTree != null)
            {
                SqlGenerator    sqlGen  = new SqlGenerator();                
                String          sql     = sqlGen.GenerateSql((DbQueryCommandTree)tree);

                parameters = sqlGen.Parameters;

                return sql;
            }

            // TODO: ENABLE UPDATE
            // Handle Insert
            DbInsertCommandTree DbInsertCommandTree = tree as DbInsertCommandTree;

            if (DbInsertCommandTree != null)
            {
                return DmlSqlGenerator.GenerateInsertSql(DbInsertCommandTree, out parameters);
            }
           
            // Handle Delete
            DbDeleteCommandTree deleteCommandTree = tree as DbDeleteCommandTree;

            if (deleteCommandTree != null)
            {
                return DmlSqlGenerator.GenerateDeleteSql(deleteCommandTree, out parameters);
            }
            
            // Handle Update
            DbUpdateCommandTree updateCommandTree = tree as DbUpdateCommandTree;

            if (updateCommandTree != null)
            {
                return DmlSqlGenerator.GenerateUpdateSql(updateCommandTree, out parameters);
            }
            
            throw new NotSupportedException("Unrecognized command tree type");
        }

        #endregion

        #region � Driver Methods �

        /// <summary>
        /// Translate a command tree to a SQL string.
        ///
        /// The input tree could be translated to either a SQL SELECT statement
        /// or a SELECT expression.  This choice is made based on the return type
        /// of the expression
        /// CollectionType => select statement
        /// non collection type => select expression
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>The string representing the SQL to be executed.</returns>
        private string GenerateSql(DbQueryCommandTree tree)
        {
            tree.Validate();

            this.selectStatementStack   = new Stack<SqlSelectStatement>();
            this.isParentAJoinStack     = new Stack<bool>();
            this.allExtentNames         = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            this.allColumnNames         = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            this.parameters             = new List<DbParameter>();

            // Literals will not be converted to parameters.
            ISqlFragment result;

            if (MetadataHelpers.IsCollectionType(tree.Query.ResultType))
            {
                SqlSelectStatement sqlStatement = this.VisitExpressionEnsureSqlStatement(tree.Query);
                
                Debug.Assert(sqlStatement != null, "The outer most sql statment is null");
                
                sqlStatement.IsTopMost  = true;
                result                  = sqlStatement;
            }
            else
            {
                SqlBuilder sqlBuilder = new SqlBuilder();

                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append(tree.Query.Accept(this));

                result = sqlBuilder;
            }

            if (this.isVarRefSingle)
            {
                throw new NotSupportedException();
                // A VariableReferenceExpression has to be a child of PropertyExpression or MethodExpression
            }

            // Check that the parameter stacks are not leaking.
            Debug.Assert(this.selectStatementStack.Count == 0);
            Debug.Assert(this.isParentAJoinStack.Count == 0);

            return this.WriteSql(result);
        }

        /// <summary>
        /// Convert the SQL fragments to a string.
        /// We have to setup the Stream for writing.
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <returns>A string representing the SQL to be executed.</returns>
        private string WriteSql(ISqlFragment sqlStatement)
        {
            StringBuilder builder = new StringBuilder(1024);

            using (SqlWriter writer = new SqlWriter(builder))
            {
                sqlStatement.WriteSql(writer, this);
            }

            return builder.ToString();
        }

        #endregion

        #region � ExpressionVisitor Members �

        /// <summary>
        /// Translate(left) AND Translate(right)
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>.</returns>
        public override ISqlFragment Visit(DbAndExpression e)
        {
            return this.VisitBinaryExpression(" AND ", e.Left, e.Right);
        }

        /// <summary>
        /// An apply is just like a join, so it shares the common join processing
        /// in <see cref="VisitJoinExpression"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/>.</returns>
        public override ISqlFragment Visit(DbApplyExpression e)
        {
            List<DbExpressionBinding>   inputs      = new List<DbExpressionBinding>();
            string                      joinString  = null;
            
            inputs.Add(e.Input);
            inputs.Add(e.Apply);

            switch (e.ExpressionKind)
            {
                case DbExpressionKind.CrossApply:
                    joinString = "CROSS APPLY";
                    break;

                case DbExpressionKind.OuterApply:
                    joinString = "OUTER APPLY";
                    break;

                default:
                    Debug.Assert(false);
                    throw new InvalidOperationException();
            }

            // The join condition does not exist in this case, so we use null.
            // We do not have a on clause, so we use JoinType.CrossJoin.
            return this.VisitJoinExpression(inputs, DbExpressionKind.CrossJoin, joinString, null);
        }

        /// <summary>
        /// For binary expressions, we delegate to <see cref="VisitBinaryExpression"/>.
        /// We handle the other expressions directly.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbArithmeticExpression e)
        {
            SqlBuilder result;

            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Divide:
                    result = this.VisitBinaryExpression(" / ", e.Arguments[0], e.Arguments[1]);
                    break;

                case DbExpressionKind.Minus:
                    result = this.VisitBinaryExpression(" - ", e.Arguments[0], e.Arguments[1]);
                    break;

                case DbExpressionKind.Modulo:
                    result = this.VisitBinaryExpression(" % ", e.Arguments[0], e.Arguments[1]);
                    break;

                case DbExpressionKind.Multiply:
                    result = this.VisitBinaryExpression(" * ", e.Arguments[0], e.Arguments[1]);
                    break;

                case DbExpressionKind.Plus:
#warning Needs to be tested
                    if (MetadataHelpers.GetDbType(MetadataHelpers.GetPrimitiveTypeKind(e.Arguments[0].ResultType)) == DbType.String ||
                        MetadataHelpers.GetDbType(MetadataHelpers.GetPrimitiveTypeKind(e.Arguments[1].ResultType)) == DbType.String)
                    {
                        result = this.VisitBinaryExpression(" || ", e.Arguments[0], e.Arguments[1]);
                    }
                    else
                    {
                        result = this.VisitBinaryExpression(" + ", e.Arguments[0], e.Arguments[1]);
                    }
                    break;

                case DbExpressionKind.UnaryMinus:
                    result = new SqlBuilder();
                    result.Append(" -(");
                    result.Append(e.Arguments[0].Accept(this));
                    result.Append(")");
                    break;

                default:
                    Debug.Assert(false);
                    throw new InvalidOperationException();
            }

            return result;
        }

        /// <summary>
        /// If the ELSE clause is null, we do not write it out.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbCaseExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            Debug.Assert(e.When.Count == e.Then.Count);

            result.Append("CASE");

            for (int i = 0; i < e.When.Count; ++i)
            {
                result.Append(" WHEN (");
                result.Append(e.When[i].Accept(this));
                result.Append(") THEN ");
                result.Append(e.Then[i].Accept(this));
            }

            if (e.Else != null && !(e.Else is DbNullExpression))
            {
                result.Append(" ELSE ");
                result.Append(e.Else.Accept(this));
            }

            result.Append(" END");

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbCastExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            result.Append(" CAST( ");
            result.Append(e.Argument.Accept(this));
            result.Append(" AS ");
            result.Append(this.GetSqlPrimitiveType(e.ResultType));
            result.Append(")");

            return result;
        }

        /// <summary>
        /// The parser generates Not(Equals(...)) for &lt;&gt;.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>.</returns>
        public override ISqlFragment Visit(DbComparisonExpression e)
        {
            SqlBuilder result;

            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Equals:
                    result = this.VisitBinaryExpression(" = ", e.Left, e.Right);
                    break;

                case DbExpressionKind.LessThan:
                    result = this.VisitBinaryExpression(" < ", e.Left, e.Right);
                    break;

                case DbExpressionKind.LessThanOrEquals:
                    result = this.VisitBinaryExpression(" <= ", e.Left, e.Right);
                    break;

                case DbExpressionKind.GreaterThan:
                    result = this.VisitBinaryExpression(" > ", e.Left, e.Right);
                    break;

                case DbExpressionKind.GreaterThanOrEquals:
                    result = this.VisitBinaryExpression(" >= ", e.Left, e.Right);
                    break;

                    // The parser does not generate the expression kind below.
                case DbExpressionKind.NotEquals:
                    result = this.VisitBinaryExpression(" <> ", e.Left, e.Right);
                    break;

                default:
                    Debug.Assert(false);  // The constructor should have prevented this
                    throw new InvalidOperationException(String.Empty);
            }

            return result;
        }

        /// <summary>
        /// Constants will be send to the store as part of the generated TSQL, not as parameters
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>.  Strings are wrapped in single
        /// quotes and escaped.  Numbers are written literally.</returns>
        public override ISqlFragment Visit(DbConstantExpression e)
        {
            SqlBuilder          result = new SqlBuilder();
            PrimitiveTypeKind   typeKind;

            // Model Types can be (at the time of this implementation):
            //      Binary, Boolean, Byte, DateTime, Decimal, Double, Guid, Int16, Int32, Int64,Single, String
            if (MetadataHelpers.TryGetPrimitiveTypeKind(e.ResultType, out typeKind))
            {
                switch (typeKind)
                {
                    case PrimitiveTypeKind.Int32: 
                        result.Append(e.Value.ToString());
                        break;

                    case PrimitiveTypeKind.Binary:
                        throw new NotSupportedException("Binary constants are not supported");
                        /*
                        result.Append(" 0x");
                        result.Append(ByteArrayToBinaryString((Byte[])e.Value));
                        result.Append(" ");
                        */
                        break;

                    case PrimitiveTypeKind.Boolean:
                        result.Append((bool)e.Value ? "cast(1 as smallint)" : "cast(0 as smallint)");
                        break;

                    case PrimitiveTypeKind.Byte:
                        result.Append("cast(");
                        result.Append(e.Value.ToString());
                        result.Append(" as smallint)");
                        break;

                    case PrimitiveTypeKind.DateTime:
                        result.Append("cast(");
                        result.Append(e.ResultType.EdmType.Name);
                        result.Append(", ");
                        result.Append(this.EscapeSingleQuote(((System.DateTime)e.Value).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), false /* IsUnicode */));
                        result.Append(" as timestamp)");
                        break;

                    case PrimitiveTypeKind.Decimal:
                        string strDecimal = ((Decimal)e.Value).ToString(CultureInfo.InvariantCulture);

                        // if the decimal value has no decimal part, cast as decimal to preserve type
                        // if the number has precision > int64 max precision, it will be handled as decimal by sql server
                        // and does not need cast. if precision is lest then 20, then cast using Max(literal precision, sql default precision)
                        if (strDecimal.IndexOf('.') == -1 && (strDecimal.TrimStart(new char[] {'-'}).Length < 20 ))
                        {
                            byte precision = (Byte)strDecimal.Length;
                            FacetDescription precisionFacetDescription;

                            Debug.Assert(MetadataHelpers.TryGetTypeFacetDescriptionByName(e.ResultType.EdmType, "precision", out precisionFacetDescription), "Decimal primitive type must have Precision facet");

                            if (MetadataHelpers.TryGetTypeFacetDescriptionByName(e.ResultType.EdmType, "precision", out precisionFacetDescription))
                            {
                                precision = Math.Max(precision, (byte)precisionFacetDescription.DefaultValue);
                            }
                            
                            Debug.Assert(precision > 0, "Precision must be greater than zero");

                            result.Append("cast(");
                            result.Append(strDecimal);
                            result.Append(" as decimal(");
                            result.Append(precision.ToString(CultureInfo.InvariantCulture));
                            result.Append("))");
                        }
                        else
                        {
                            result.Append(strDecimal);
                        }
                        break;

                    case PrimitiveTypeKind.Double:
                        result.Append("cast(");
                        result.Append(((Double)e.Value).ToString(CultureInfo.InvariantCulture));
                        result.Append(" as double precision)");
                        break;

                    case PrimitiveTypeKind.Guid:
                        throw new NotSupportedException("Guid constants are not supported");
                        /*
                        result.Append("cast(");
                        result.Append(EscapeSingleQuote(e.Value.ToString(), false));
                        result.Append(" as uniqueidentifier)");
                        */
                        break;

                    case PrimitiveTypeKind.Int16:
                        result.Append("cast(");
                        result.Append(e.Value.ToString());
                        result.Append(" as smallint)");
                        break;

                    case PrimitiveTypeKind.Int64:
                        result.Append("cast(");
                        result.Append(e.Value.ToString());
                        result.Append(" as bigint)");
                        break;

                    case PrimitiveTypeKind.Single:
                        result.Append("cast(");
                        result.Append(((Single)e.Value).ToString(CultureInfo.InvariantCulture));
                        result.Append(" as float)");
                        break;

                    case PrimitiveTypeKind.String:
                        bool isUnicode = MetadataHelpers.GetFacetValueOrDefault<bool>(e.ResultType, MetadataHelpers.UnicodeFacetName);
                        result.Append(EscapeSingleQuote(e.Value as string, isUnicode));
                        break;

                    default:
                        // all known scalar types should been handled already.
                        throw new NotSupportedException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            return result;

        }

        /// <summary>
        /// <see cref="DerefExpression"/> is illegal at this stage
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbDerefExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The DISTINCT has to be added to the beginning of SqlSelectStatement.Select,
        /// but it might be too late for that.  So, we use a flag on SqlSelectStatement
        /// instead, and add the "DISTINCT" in the second phase.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        public override ISqlFragment Visit(DbDistinctExpression e)
        {
            SqlSelectStatement result = VisitExpressionEnsureSqlStatement(e.Argument);

            if (!IsCompatible(result, e.ExpressionKind))
            {
                Symbol      fromSymbol;
                TypeUsage   inputType = MetadataHelpers.GetElementTypeUsage(e.Argument.ResultType);

                result = CreateNewSelectStatement(result, "distinct", inputType, out fromSymbol);

                this.AddFromSymbol(result, "distinct", fromSymbol, false);
            }
            
            result.IsDistinct = true;

            return result;
        }

        /// <summary>
        /// An element expression returns a scalar - so it is translated to
        /// ( Select ... )
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbElementExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            result.Append("(");
            result.Append(this.VisitExpressionEnsureSqlStatement(e.Argument));
            result.Append(")");

            return result;
        }

        /// <summary>
        /// <see cref="Visit(UnionAllExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbExceptExpression e)
        {        
            return this.VisitSetOpExpression(e.Left, e.Right, "EXCEPT");
        }

        /// <summary>
        /// Only concrete expression types will be visited.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbExpression e)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>If we are in a Join context, returns a <see cref="SqlBuilder"/>
        /// with the extent name, otherwise, a new <see cref="SqlSelectStatement"/>
        /// with the From field set.</returns>
        public override ISqlFragment Visit(DbScanExpression e)
        {
            EntitySetBase target = e.Target;
                        
            if (IsParentAJoin)
            {
                SqlBuilder result = new SqlBuilder();
                result.Append(GetTargetSql(target));

                return result;
            }
            else
            {
                SqlSelectStatement result = new SqlSelectStatement();
                result.From.Append(GetTargetSql(target));

                return result;
            }
        }

        /// <summary>
        /// Gets escaped TSql identifier describing this entity set.
        /// </summary>
        /// <returns></returns>
        internal static string GetTargetSql(EntitySetBase entitySetBase)
        {
             // construct escaped T-SQL referencing entity set
            StringBuilder builder = new StringBuilder(50);         
            string definingQuery = MetadataHelpers.TryGetValueForMetadataProperty<string>(entitySetBase, "DefiningQuery");

            if (!string.IsNullOrEmpty(definingQuery))
            {
                builder.Append("(");
                builder.Append(definingQuery);
                builder.Append(")");
            }
            else
            {
#warning Firebird does not support Schemas
                //string schemaName = MetadataHelpers.TryGetValueForMetadataProperty<string>(entitySetBase, "Schema");

                //if (!string.IsNullOrEmpty(schemaName))
                //{
                //    builder.Append(SqlGenerator.QuoteIdentifier(schemaName));
                //    builder.Append(".");
                //}
                //else
                //{
                //    builder.Append(SqlGenerator.QuoteIdentifier(entitySetBase.EntityContainer.Name));
                //    builder.Append(".");
                //}

                string tableName = MetadataHelpers.TryGetValueForMetadataProperty<string>(entitySetBase, "Table");

                if (!string.IsNullOrEmpty(tableName))
                {
                    builder.Append(SqlGenerator.QuoteIdentifier(tableName));
                }
                else
                {
                    builder.Append(SqlGenerator.QuoteIdentifier(entitySetBase.Name));
                }
            }

            return builder.ToString();
        }


        /// <summary>
        /// <see cref="ViewExpression"/> is illegal at this stage
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbViewExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The bodies of <see cref="Visit(FilterExpression)"/>, <see cref="Visit(GroupByExpression)"/>,
        /// <see cref="Visit(ProjectExpression)"/>, <see cref="Visit(SortExpression)"/> are similar.
        /// Each does the following.
        /// <list type="number">
        /// <item> Visit the input expression</item>
        /// <item> Determine if the input's SQL statement can be reused, or a new
        /// one must be created.</item>
        /// <item>Create a new symbol table scope</item>
        /// <item>Push the Sql statement onto a stack, so that children can
        /// update the free variable list.</item>
        /// <item>Visit the non-input expression.</item>
        /// <item>Cleanup</item>
        /// </list>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        public override ISqlFragment Visit(DbFilterExpression e)
        {
            return VisitFilterExpression(e.Input, e.Predicate, false);
        }

        /// <summary>
        /// Lambda functions are not supported.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbFunctionExpression e)
        {
            if (e.IsLambda)
            {
                throw new NotSupportedException();
            }

            SqlBuilder result = new SqlBuilder();

            //
            // check if function requires special case processing, if so, delegates to it
            //
            if (this.IsSpecialFunction(e))
            {
                return this.SpecialFunctionHandler(e);
            }

            //
            // otherwise, handle as ordinary function
            //
            this.WriteFunctionName(result, e.Function);

            bool isNiladicFunction = MetadataHelpers.TryGetValueForMetadataProperty<bool>(e.Function, "NiladicFunctionAttribute");

            if (isNiladicFunction && e.Arguments.Count > 0)
            {
                throw new InvalidOperationException("Niladic functions cannot have parameters");
            }

            if (!isNiladicFunction)
            {
                result.Append("(");
                string separator = "";

                foreach (DbExpression arg in e.Arguments)
                {
                    result.Append(separator);
                    result.Append(arg.Accept(this));
                    separator = ", ";
                }

                result.Append(")");
            }

            return result;
        }

        /// <summary>
        /// returns true if a given function requires special case handling
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsSpecialFunction(DbFunctionExpression e)
        {
            return FunctionHandlers.ContainsKey(e.Function.Name.ToLowerInvariant());
        }

        /// <summary>
        /// dispatches the special function processing to the appropriate handler.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private ISqlFragment SpecialFunctionHandler( DbFunctionExpression e )
        {
            FunctionHandler fnHandler = null;

            if (FunctionHandlers.TryGetValue(e.Function.Name.ToLowerInvariant(), out fnHandler))
            {
                return fnHandler(this, e);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Handles transient function concat. it expands the operation as native TSQL '+' op
        /// over string types.
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleConcatFunction( SqlGenerator sqlgen, DbFunctionExpression e )
        {
            // 
            // Canonical functions will turn this logic unnecessary
            // 
            SqlBuilder result = new SqlBuilder();

            Debug.Assert(e.Arguments.Count == 2, "Concat() e.Arguments.Count == 2");

            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(" + ");
            result.Append(e.Arguments[1].Accept(sqlgen));

            return result;
        }

        /// <summary>
        /// Handles special case in which datapart 'type' parameter is present. all the functions
        /// handles here have *only* the 1st parameter as datepart. datepart value is passed along
        /// the QP as string and has to be expanded as TSQL keyword.
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleDatepartDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            Debug.Assert(e.Arguments.Count > 0, "e.Arguments.Count > 0");

            DbConstantExpression constExpr = e.Arguments[0] as DbConstantExpression;

            if (null == constExpr)
            {
                throw new InvalidOperationException(String.Format("DATEPART argument to function '{0}.{1}' must be a literal string", e.Function.NamespaceName, e.Function.Name));
            }

            string datepart = constExpr.Value as string;
            if (null == datepart)
            {
                throw new InvalidOperationException(String.Format("DATEPART argument to function '{0}.{1}' must be a literal string", e.Function.NamespaceName, e.Function.Name));
            }

            SqlBuilder result = new SqlBuilder();

            //
            // check if datepart value is valid
            //
            if (!DatepartKeywords.ContainsKey(datepart.ToLowerInvariant()))
            {
                throw new InvalidOperationException(String.Format("{0}' is not a valid value for DATEPART argument in '{1}.{2}' function", datepart, e.Function.NamespaceName, e.Function.Name));
            }

            //
            // finaly, expand the function name
            //
            sqlgen.WriteFunctionName(result, e.Function);
            result.Append("(");

            // expand the datepart literal as tsql kword
            result.Append(datepart);
            string separator = ", ";

            // expand remaining arguments
            for (int i = 1 ; i < e.Arguments.Count ; i++)
            {
                result.Append(separator);
                result.Append(e.Arguments[i].Accept(sqlgen));
            }

            result.Append(")");

            return result;
        }

        /// <summary>
        /// <see cref="EntityRefExpression"/> is illegal at this stage
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbEntityRefExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="RefKeyExpression"/> is illegal at this stage
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbRefKeyExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="Visit(FilterExpression)"/> for general details.
        /// We modify both the GroupBy and the Select fields of the SqlSelectStatement.
        /// GroupBy gets just the keys without aliases,
        /// and Select gets the keys and the aggregates with aliases.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        public override ISqlFragment Visit(DbGroupByExpression e)
        {
            Symbol              fromSymbol;
            SqlSelectStatement  result = this.VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // GroupBy is compatible with Filter and OrderBy
            // but not with Project, GroupBy
            if (!this.IsCompatible(result, e.ExpressionKind))
            {
                result = this.CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            this.selectStatementStack.Push(result);
            this.symbolTable.EnterScope();

            this.AddFromSymbol(result, e.Input.VariableName, fromSymbol);

            // This line is not present for other relational nodes.
            this.symbolTable.Add(e.Input.GroupVariableName, fromSymbol);


            // The enumerator is shared by both the keys and the aggregates,
            // so, we do not close it in between.
            RowType groupByType = MetadataHelpers.GetEdmType<RowType>(MetadataHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage);

            using (IEnumerator<EdmProperty> members = groupByType.Properties.GetEnumerator())
            {
                members.MoveNext();
                Debug.Assert(result.Select.IsEmpty);

                string separator = "";

                foreach (DbExpression key in e.Keys)
                {
                    EdmProperty member = members.Current;
                    result.GroupBy.Append(separator);
                    result.Select.Append(separator);

                    ISqlFragment keySql = key.Accept(this);
                    result.GroupBy.Append(keySql);
                    result.Select.Append(keySql);
                    result.Select.Append(" AS ");
                    result.Select.Append(QuoteIdentifier(member.Name));

                    separator = ", ";
                    members.MoveNext();
                }

                foreach (DbAggregate aggregate in e.Aggregates)
                {
                    EdmProperty member = members.Current;
                    result.Select.Append(separator);

                    result.Select.Append(this.VisitAggregate(aggregate));
                    result.Select.Append(" AS ");
                    result.Select.Append(QuoteIdentifier(member.Name));
                    separator = ", ";
                    members.MoveNext();
                }
            }

            this.symbolTable.ExitScope();
            this.selectStatementStack.Pop();

            return result;
        }

        /// <summary>
        /// <see cref="Visit(UnionAllExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbIntersectExpression e)
        {
            return this.VisitSetOpExpression(e.Left, e.Right, "INTERSECT");
        }

        /// <summary>
        /// Not(IsEmpty) has to be handled specially, so we delegate to
        /// <see cref="VisitIsEmptyExpression"/>.
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>.
        /// <code>[NOT] EXISTS( ... )</code>
        /// </returns>
        public override ISqlFragment Visit(DbIsEmptyExpression e)
        {
            return this.VisitIsEmptyExpression(e, false);
        }

        /// <summary>
        /// Not(IsNull) is handled specially, so we delegate to
        /// <see cref="VisitIsNullExpression"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/>
        /// <code>IS [NOT] NULL</code>
        /// </returns>
        public override ISqlFragment Visit(DbIsNullExpression e)
        {
            return this.VisitIsNullExpression(e, false);
        }

        /// <summary>
        /// <see cref="IsOfExpression"/> is illegal at this stage
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbIsOfExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="VisitJoinExpression"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/>.</returns>
        public override ISqlFragment Visit(DbCrossJoinExpression e)
        {
            return this.VisitJoinExpression(e.Inputs, e.ExpressionKind, "CROSS JOIN", null);
        }

        /// <summary>
        /// <see cref="VisitJoinExpression"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/>.</returns>
        public override ISqlFragment Visit(DbJoinExpression e)
        {
            string joinString;

            switch (e.ExpressionKind)
            {
                case DbExpressionKind.FullOuterJoin:
                    joinString = "FULL OUTER JOIN";
                    break;

                case DbExpressionKind.InnerJoin:
                    joinString = "INNER JOIN";
                    break;

                case DbExpressionKind.LeftOuterJoin:
                    joinString = "LEFT OUTER JOIN";
                    break;

                default:
                    Debug.Assert(false);
                    joinString = null;
                    break;
            }

            List<DbExpressionBinding> inputs = new List<DbExpressionBinding>(2);

            inputs.Add(e.Left);
            inputs.Add(e.Right);

            return this.VisitJoinExpression(inputs, e.ExpressionKind, joinString, e.JoinCondition);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbLikeExpression e)
        {
            SqlBuilder result = new SqlBuilder();
            result.Append(e.Argument.Accept(this));
            result.Append(" LIKE ");
            result.Append(e.Pattern.Accept(this));

            // if the ESCAPE expression is a NullExpression, then that's tantamount to 
            // not having an ESCAPE at all
            if (e.Escape.ExpressionKind != DbExpressionKind.Null)
            {
                result.Append(" ESCAPE ");
                result.Append(e.Escape.Accept(this));
            }

            return result;
        }

        /// <summary>
        ///  Translates to TOP expression.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbLimitExpression e)
        {
            Debug.Assert(e.Limit is DbConstantExpression || e.Limit is DbParameterReferenceExpression, "LimitExpression.Limit is of invalid expression type");

            SqlSelectStatement  result      = this.VisitExpressionEnsureSqlStatement(e.Argument, false);
            Symbol              fromSymbol  = null;

            if (!this.IsCompatible(result, e.ExpressionKind))
            {
                TypeUsage inputType = MetadataHelpers.GetElementTypeUsage(e.Argument.ResultType);

                result = this.CreateNewSelectStatement(result, "first", inputType, out fromSymbol);
                this.AddFromSymbol(result, "first", fromSymbol, false);
            }

            ISqlFragment topCount = this.HandleCountExpression(e.Limit);                       

            result.Top = new FirstClause(topCount);

            return result;
        }

        /// <summary>
        ///  Translates to SKIP expression.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbSkipExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Translate a NewInstance(Element(X)) expression into
        ///   "select top(1) * from X"
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private ISqlFragment VisitCollectionConstructor(DbNewInstanceExpression e)
        {
            Debug.Assert(e.Arguments.Count <= 1);

            if (e.Arguments.Count == 1 && e.Arguments[0].ExpressionKind == DbExpressionKind.Element)
            {
                DbElementExpression elementExpr = e.Arguments[0] as DbElementExpression;
                SqlSelectStatement  result      = this.VisitExpressionEnsureSqlStatement(elementExpr.Argument);

                if (!this.IsCompatible(result, DbExpressionKind.Element))
                {
                    Symbol fromSymbol;
                    TypeUsage inputType = MetadataHelpers.GetElementTypeUsage(elementExpr.Argument.ResultType);

                    result = CreateNewSelectStatement(result, "element", inputType, out fromSymbol);
                    AddFromSymbol(result, "element", fromSymbol, false);
                }

                result.Top = new FirstClause(1);

                return result;
            }


            // Otherwise simply build this out as a union-all ladder
            CollectionType collectionType = MetadataHelpers.GetEdmType<CollectionType>(e.ResultType);
            Debug.Assert(collectionType != null);

            bool isScalarElement = MetadataHelpers.IsPrimitiveType(collectionType.TypeUsage);

            SqlBuilder  resultSql = new SqlBuilder();
            string      separator = "";

            // handle empty table
            if (e.Arguments.Count == 0)
            {
                Debug.Assert(isScalarElement);

                resultSql.Append(" SELECT CAST(null as ");
                resultSql.Append(this.GetSqlPrimitiveType(collectionType.TypeUsage));
                resultSql.Append(") AS X FROM (SELECT 1) AS Y WHERE 1=0");
            }

            foreach (DbExpression arg in e.Arguments)
            {
                resultSql.Append(separator);
                resultSql.Append(" SELECT ");
                resultSql.Append(arg.Accept(this));

                // For scalar elements, no alias is appended yet. Add this.
                if (isScalarElement)
                {
                    resultSql.Append(" AS X ");
                }

                separator = " UNION ALL ";
            }

            return resultSql;
        }

        /// <summary>
        /// NewInstanceExpression is allowed as a child of ProjectExpression only.
        /// If anyone else is the parent, we throw.
        /// We also perform special casing for collections - where we could convert
        /// them into Unions
        ///
        /// <see cref="VisitNewInstanceExpression"/> for the actual implementation.
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbNewInstanceExpression e)
        {
            if (MetadataHelpers.IsCollectionType(e.ResultType))
            {
                return VisitCollectionConstructor(e);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// The Not expression may cause the translation of its child to change.
        /// These children are
        /// <list type="bullet">
        /// <item><see cref="NotExpression"/>NOT(Not(x)) becomes x</item>
        /// <item><see cref="IsEmptyExpression"/>NOT EXISTS becomes EXISTS</item>
        /// <item><see cref="IsNullExpression"/>IS NULL becomes IS NOT NULL</item>
        /// <item><see cref="ComparisonExpression"/>= becomes&lt;&gt; </item>
        /// </list>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbNotExpression e)
        {
            // Flatten Not(Not(x)) to x.
            DbNotExpression notExpression = e.Argument as DbNotExpression;

            if (notExpression != null)
            {
                return notExpression.Argument.Accept(this);
            }

            DbIsEmptyExpression isEmptyExpression = e.Argument as DbIsEmptyExpression;

            if (isEmptyExpression != null)
            {
                return VisitIsEmptyExpression(isEmptyExpression, true);
            }

            DbIsNullExpression isNullExpression = e.Argument as DbIsNullExpression;

            if (isNullExpression != null)
            {
                return VisitIsNullExpression(isNullExpression, true);
            }

            DbComparisonExpression comparisonExpression = e.Argument as DbComparisonExpression;

            if (comparisonExpression != null)
            {
                if (comparisonExpression.ExpressionKind == DbExpressionKind.Equals)
                {
                    return VisitBinaryExpression(" <> ", comparisonExpression.Left, comparisonExpression.Right);
                }
            }

            SqlBuilder result = new SqlBuilder();

            result.Append(" NOT (");
            result.Append(e.Argument.Accept(this));
            result.Append(")");

            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        /// <returns><see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbNullExpression e)
        {
            SqlBuilder  result  = new SqlBuilder();
            TypeUsage   type    = e.ResultType;

            // always cast nulls - sqlserver doesn't like case expressions where the "then" clause is null
            result.Append("CAST(NULL AS ");            
            result.Append(this.GetSqlPrimitiveType(type));
            result.Append(")");
            
            return result;
        }

        /// <summary>
        /// <see cref="OfTypeExpression"/> is illegal at this stage
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbOfTypeExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        /// <seealso cref="Visit(AndExpression)"/>
        public override ISqlFragment Visit(DbOrExpression e)
        {
            return this.VisitBinaryExpression(" OR ", e.Left, e.Right);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbParameterReferenceExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            // Do not quote this name.
            // We are not checking that e.Name has no illegal characters. e.g. space
            result.Append("@" + e.ParameterName);

            // Create the DbParameter instance
            PrimitiveTypeKind   primitiveType   = MetadataHelpers.GetPrimitiveTypeKind(e.ResultType);
            DbType              dbType          = MetadataHelpers.GetDbType(primitiveType);

            DbParameter parameter = new FbParameter();

            parameter.ParameterName = e.ParameterName;
            parameter.DbType        = dbType;
            
            this.parameters.Add(parameter);

            return result;
        }

        /// <summary>
        /// <see cref="Visit(FilterExpression)"/> for the general ideas.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        /// <seealso cref="Visit(FilterExpression)"/>
        public override ISqlFragment Visit(DbProjectExpression e)
        {
            Symbol              fromSymbol;
            SqlSelectStatement  result = this.VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // Project is compatible with Filter
            // but not with Project, GroupBy
            if (!this.IsCompatible(result, e.ExpressionKind))
            {
                result = this.CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            this.selectStatementStack.Push(result);
            this.symbolTable.EnterScope();

            this.AddFromSymbol(result, e.Input.VariableName, fromSymbol);

            // Project is the only node that can have NewInstanceExpression as a child
            // so we have to check it here.
            // We call VisitNewInstanceExpression instead of Visit(NewInstanceExpression), since
            // the latter throws.
            DbNewInstanceExpression newInstanceExpression = e.Projection as DbNewInstanceExpression;

            if (newInstanceExpression != null)
            {
                result.Select.Append(this.VisitNewInstanceExpression(newInstanceExpression));
            }
            else
            {
                result.Select.Append(e.Projection.Accept(this));
            }

            this.symbolTable.ExitScope();
            this.selectStatementStack.Pop();

            return result;
        }

        /// <summary>
        /// This method handles record flattening, which works as follows.
        /// consider an expression <c>Prop(y, Prop(x, Prop(d, Prop(c, Prop(b, Var(a)))))</c>
        /// where a,b,c are joins, d is an extent and x and y are fields.
        /// b has been flattened into a, and has its own SELECT statement.
        /// c has been flattened into b.
        /// d has been flattened into c.
        ///
        /// We visit the instance, so we reach Var(a) first.  This gives us a (join)symbol.
        /// Symbol(a).b gives us a join symbol, with a SELECT statement i.e. Symbol(b).
        /// From this point on , we need to remember Symbol(b) as the source alias,
        /// and then try to find the column.  So, we use a SymbolPair.
        ///
        /// We have reached the end when the symbol no longer points to a join symbol.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="JoinSymbol"/> if we have not reached the first
        /// Join node that has a SELECT statement.
        /// A <see cref="SymbolPair"/> if we have seen the JoinNode, and it has
        /// a SELECT statement.
        /// A <see cref="SqlBuilder"/> with {Input}.propertyName otherwise.
        /// </returns>
        public override ISqlFragment Visit(DbPropertyExpression e)
        {
            SqlBuilder      result;
            ISqlFragment    instanceSql = e.Instance.Accept(this);

            // Since the VariableReferenceExpression is a proper child of ours, we can reset
            // isVarSingle.
            DbVariableReferenceExpression variableReferenceExpression = e.Instance as DbVariableReferenceExpression;

            if (variableReferenceExpression != null)
            {
                this.isVarRefSingle = false;
            }

            // We need to flatten, and have not yet seen the first nested SELECT statement.
            JoinSymbol joinSymbol = instanceSql as JoinSymbol;

            if (joinSymbol != null)
            {
                Debug.Assert(joinSymbol.NameToExtent.ContainsKey(e.Property.Name));

                if (joinSymbol.IsNestedJoin)
                {
                    return new SymbolPair(joinSymbol, joinSymbol.NameToExtent[e.Property.Name]);
                }
                else
                {
                    return joinSymbol.NameToExtent[e.Property.Name];
                }
            }

            // ---------------------------------------
            // We have seen the first nested SELECT statement, but not the column.
            SymbolPair symbolPair = instanceSql as SymbolPair;

            if (symbolPair != null)
            {
                JoinSymbol columnJoinSymbol = symbolPair.Column as JoinSymbol;
                if (columnJoinSymbol != null)
                {
                    symbolPair.Column = columnJoinSymbol.NameToExtent[e.Property.Name];
                    return symbolPair;
                }
                else
                {
                    // symbolPair.Column has the base extent.
                    // we need the symbol for the column, since it might have been renamed
                    // when handling a JOIN.
                    if (symbolPair.Column.Columns.ContainsKey(e.Property.Name))
                    {
                        result = new SqlBuilder();
                        result.Append(symbolPair.Source);
                        result.Append(".");
                        result.Append(symbolPair.Column.Columns[e.Property.Name]);
                        return result;
                    }
                }
            }
            // ---------------------------------------

            result = new SqlBuilder();
            result.Append(instanceSql);
            result.Append(".");

            // At this point the column name cannot be renamed, so we do
            // not use a symbol.
            result.Append(QuoteIdentifier(e.Property.Name));
            
            return result;
        }

        /// <summary>
        /// Any(input, x) => Exists(Filter(input,x))
        /// All(input, x) => Not Exists(Filter(input, not(x))
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbQuantifierExpression e)
        {
            SqlBuilder  result          = new SqlBuilder();
            bool        negatePredicate = (e.ExpressionKind == DbExpressionKind.All);

            if (e.ExpressionKind == DbExpressionKind.Any)
            {
                result.Append("EXISTS (");
            }
            else
            {
                Debug.Assert(e.ExpressionKind == DbExpressionKind.All);
                result.Append("NOT EXISTS (");
            }

            SqlSelectStatement filter = this.VisitFilterExpression(e.Input, e.Predicate, negatePredicate);
            if (filter.Select.IsEmpty)
            {
                AddDefaultColumns(filter);
            }

            result.Append(filter);
            result.Append(")");

            return result;
        }

        /// <summary>
        /// <see cref="RefExpression"/> is illegal at this stage
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbRefExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="RelationshipNavigationExpression"/> is illegal at this stage
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbRelationshipNavigationExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <see cref="Visit(FilterExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlSelectStatement"/></returns>
        /// <seealso cref="Visit(FilterExpression)"/>
        public override ISqlFragment Visit(DbSortExpression e)
        {
            Symbol              fromSymbol;
            SqlSelectStatement  result = this.VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // OrderBy is compatible with Filter
            // and nothing else
            if (!this.IsCompatible(result, e.ExpressionKind))
            {
                result = this.CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            this.selectStatementStack.Push(result);
            this.symbolTable.EnterScope();

            this.AddFromSymbol(result, e.Input.VariableName, fromSymbol);

            this.AddSortKeys(result.OrderBy, e.SortOrder);

            this.symbolTable.ExitScope();
            this.selectStatementStack.Pop();

            return result;
        }

        /// <summary>
        /// <see cref="TreatExpression"/> is illegal at this stage
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        public override ISqlFragment Visit(DbTreatExpression e)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This code is shared by <see cref="Visit(ExceptExpression)"/>
        /// and <see cref="Visit(IntersectExpression)"/>
        ///
        /// <see cref="VisitSetOpExpression"/>
        /// Since the left and right expression may not be Sql select statements,
        /// we must wrap them up to look like SQL select statements.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override ISqlFragment Visit(DbUnionAllExpression e)
        {
            return this.VisitSetOpExpression(e.Left, e.Right, "UNION ALL");
        }

        /// <summary>
        /// This method determines whether an extent from an outer scope(free variable)
        /// is used in the CurrentSelectStatement.
        ///
        /// An extent in an outer scope, if its symbol is not in the FromExtents
        /// of the CurrentSelectStatement.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="Symbol"/>.</returns>
        public override ISqlFragment Visit(DbVariableReferenceExpression e)
        {
            if (this.isVarRefSingle)
            {
                throw new NotSupportedException();
                // A VariableReferenceExpression has to be a child of PropertyExpression or MethodExpression
                // This is also checked in GenerateSql(...) at the end of the visiting.
            }

            this.isVarRefSingle = true; // This will be reset by PropertyExpression or MethodExpression

            Symbol result = this.symbolTable.Lookup(e.VariableName);

            if (!this.CurrentSelectStatement.FromExtents.Contains(result))
            {
                this.CurrentSelectStatement.OuterExtents[result] = true;
            }

            return result;
        }

        #endregion

        #region � Visits shared by multiple nodes �

        /// <summary>
        /// Aggregates are not visited by the normal visitor walk.
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        private ISqlFragment VisitAggregate(DbAggregate aggregate)
        {
            SqlBuilder          result              = new SqlBuilder();
            DbFunctionAggregate functionAggregate   = aggregate as DbFunctionAggregate;
            string              separator           = "";

            if (functionAggregate != null)
            {
                this.WriteFunctionName(result, functionAggregate.Function);

                result.Append("(");

                DbFunctionAggregate fnAggr = aggregate as DbFunctionAggregate;

                if ((null != fnAggr) && (fnAggr.Distinct))
                {
                    result.Append("DISTINCT ");
                }                

                foreach (DbExpression argument in functionAggregate.Arguments)
                {
                    result.Append(separator);
                    result.Append(argument.Accept(this));
                    separator = ", ";
                }

                result.Append(")");
            }
            else
            {
                throw new NotSupportedException(); // NestAggregate.
            }

            return result;
        }

        private SqlBuilder VisitBinaryExpression(string op, DbExpression left, DbExpression right)
        {
            SqlBuilder result = new SqlBuilder();

            if (this.IsComplexExpression(left))
            {
                result.Append("(");
            }

            result.Append(left.Accept(this));

            if (this.IsComplexExpression(left))
            {
                result.Append(")");
            }

            result.Append(op);

            if (this.IsComplexExpression(right))
            {
                result.Append("(");
            }

            result.Append(right.Accept(this));

            if (this.IsComplexExpression(right))
            {
                result.Append(")");
            }

            return result;
        }

        /// <summary>
        /// This is called by the relational nodes.  It does the following
        /// <list>
        /// <item>If the input is not a SqlSelectStatement, it assumes that the input
        /// is a collection expression, and creates a new SqlSelectStatement </item>
        /// </list>
        /// </summary>
        /// <param name="inputExpression"></param>
        /// <param name="inputVarName"></param>
        /// <param name="inputVarType"></param>
        /// <param name="fromSymbol"></param>
        /// <returns>A <see cref="SqlSelectStatement"/> and the main fromSymbol
        /// for this select statement.</returns>
        private SqlSelectStatement VisitInputExpression(
            DbExpression    inputExpression, 
            string          inputVarName, 
            TypeUsage       inputVarType, 
            out Symbol      fromSymbol)
        {
            ISqlFragment        sqlFragment = inputExpression.Accept(this);
            SqlSelectStatement  result      = sqlFragment as SqlSelectStatement;

            if (result == null)
            {
                result = new SqlSelectStatement();
                this.WrapNonQueryExtent(result, sqlFragment, inputExpression.ExpressionKind);
            }

            if (result.FromExtents.Count == 0)
            {
                // input was an extent
                fromSymbol = new Symbol(inputVarName, inputVarType);
            }
            else if (result.FromExtents.Count == 1)
            {
                // input was Filter/GroupBy/Project/OrderBy
                // we are likely to reuse this statement.
                fromSymbol = result.FromExtents[0];
            }
            else
            {
                // input was a join.
                // we are reusing the select statement produced by a Join node
                // we need to remove the original extents, and replace them with a
                // new extent with just the Join symbol.
                JoinSymbol joinSymbol = new JoinSymbol(inputVarName, inputVarType, result.FromExtents);
                
                joinSymbol.FlattenedExtentList = result.AllJoinExtents;

                fromSymbol = joinSymbol;
                result.FromExtents.Clear();
                result.FromExtents.Add(fromSymbol);
            }

            return result;
        }

        /// <summary>
        /// <see cref="Visit(IsEmptyExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <param name="negate">Was the parent a NotExpression?</param>
        /// <returns></returns>
        private SqlBuilder VisitIsEmptyExpression(DbIsEmptyExpression e, bool negate)
        {
            SqlBuilder result = new SqlBuilder();

            if (!negate)
            {
                result.Append(" NOT");
            }

            result.Append(" EXISTS (");
            result.Append(this.VisitExpressionEnsureSqlStatement(e.Argument));
            result.AppendLine();
            result.Append(")");

            return result;
        }

        /// <summary>
        /// <see cref="Visit(IsNullExpression)"/>
        /// </summary>
        /// <param name="e"></param>
        /// <param name="negate">Was the parent a NotExpression?</param>
        /// <returns></returns>
        private SqlBuilder VisitIsNullExpression(DbIsNullExpression e, bool negate)
        {
            SqlBuilder result = new SqlBuilder();

            result.Append(e.Argument.Accept(this));

            if (!negate)
            {
                result.Append(" IS NULL");
            }
            else
            {
                result.Append(" IS NOT NULL");
            }

            return result;
        }

        /// <summary>
        /// This handles the processing of join expressions.
        /// The extents on a left spine are flattened, while joins
        /// not on the left spine give rise to new nested sub queries.
        ///
        /// Joins work differently from the rest of the visiting, in that
        /// the parent (i.e. the join node) creates the SqlSelectStatement
        /// for the children to use.
        ///
        /// The "parameter" IsInJoinContext indicates whether a child extent should
        /// add its stuff to the existing SqlSelectStatement, or create a new SqlSelectStatement
        /// By passing true, we ask the children to add themselves to the parent join,
        /// by passing false, we ask the children to create new Select statements for
        /// themselves.
        ///
        /// This method is called from <see cref="Visit(ApplyExpression)"/> and
        /// <see cref="Visit(JoinExpression)"/>.
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="joinKind"></param>
        /// <param name="joinString"></param>
        /// <param name="joinCondition"></param>
        /// <returns> A <see cref="SqlSelectStatement"/></returns>
        ISqlFragment VisitJoinExpression(IList<DbExpressionBinding> inputs, DbExpressionKind joinKind,
            string joinString, DbExpression joinCondition)
        {
            SqlSelectStatement result;

            // If the parent is not a join( or says that it is not),
            // we should create a new SqlSelectStatement.
            // otherwise, we add our child extents to the parent's FROM clause.
            if (!IsParentAJoin)
            {
                result = new SqlSelectStatement();

                result.AllJoinExtents = new List<Symbol>();
                selectStatementStack.Push(result);
            }
            else
            {
                result = this.CurrentSelectStatement;
            }

            // Process each of the inputs, and then the joinCondition if it exists.
            // It would be nice if we could call VisitInputExpression - that would
            // avoid some code duplication
            // but the Join postprocessing is messy and prevents this reuse.
            this.symbolTable.EnterScope();

            string  separator       = "";
            bool    isLeftMostInput = true;
            int     inputCount      = inputs.Count;

            for(int idx = 0; idx < inputCount; idx++)
            {
                DbExpressionBinding input = inputs[idx];

                if (separator != "")
                {
                    result.From.AppendLine();
                }

                result.From.Append(separator + " ");

                // Change this if other conditions are required
                // to force the child to produce a nested SqlStatement.
                bool needsJoinContext = (input.Expression.ExpressionKind == DbExpressionKind.Scan)
                                        || (isLeftMostInput && (this.IsJoinExpression(input.Expression)
                                        || this.IsApplyExpression(input.Expression)));

                this.isParentAJoinStack.Push(needsJoinContext ? true : false);

                // if the child reuses our select statement, it will append the from
                // symbols to our FromExtents list.  So, we need to remember the
                // start of the child's entries.
                int fromSymbolStart = result.FromExtents.Count;

                ISqlFragment fromExtentFragment = input.Expression.Accept(this);

                this.isParentAJoinStack.Pop();

                this.ProcessJoinInputResult(fromExtentFragment, result, input, fromSymbolStart);
                separator = joinString;

                isLeftMostInput = false;
            }

            // Visit the on clause/join condition.
            switch (joinKind)
            {
                case DbExpressionKind.FullOuterJoin:
                case DbExpressionKind.InnerJoin:
                case DbExpressionKind.LeftOuterJoin:
                    result.From.Append(" ON ");
                    this.isParentAJoinStack.Push(false);
                    result.From.Append(joinCondition.Accept(this));
                    this.isParentAJoinStack.Pop();
                    break;
            }

            this.symbolTable.ExitScope();

            if (!IsParentAJoin)
            {
                this.selectStatementStack.Pop();
            }

            return result;
        }

        /// <summary>
        /// This is called from <see cref="VisitJoinExpression"/>.
        ///
        /// This is responsible for maintaining the symbol table after visiting
        /// a child of a join expression.
        ///
        /// The child's sql statement may need to be completed.
        ///
        /// The child's result could be one of
        /// <list type="number">
        /// <item>The same as the parent's - this is treated specially.</item>
        /// <item>A sql select statement, which may need to be completed</item>
        /// <item>An extent - just copy it to the from clause</item>
        /// <item>Anything else (from a collection-valued expression) -
        /// unnest and copy it.</item>
        /// </list>
        ///
        /// If the input was a Join, we need to create a new join symbol,
        /// otherwise, we create a normal symbol.
        ///
        /// We then call AddFromSymbol to add the AS clause, and update the symbol table.
        ///
        ///
        ///
        /// If the child's result was the same as the parent's, we have to clean up
        /// the list of symbols in the FromExtents list, since this contains symbols from
        /// the children of both the parent and the child.
        /// The happens when the child visited is a Join, and is the leftmost child of
        /// the parent.
        /// </summary>
        /// <param name="fromExtentFragment"></param>
        /// <param name="result"></param>
        /// <param name="input"></param>
        /// <param name="fromSymbolStart"></param>
        private void ProcessJoinInputResult(
            ISqlFragment        fromExtentFragment, 
            SqlSelectStatement  result,
            DbExpressionBinding input, 
            int                 fromSymbolStart)
        {
            Symbol fromSymbol = null;

            if (result != fromExtentFragment)
            {
                // The child has its own select statement, and is not reusing
                // our select statement.
                // This should look a lot like VisitInputExpression().
                SqlSelectStatement sqlSelectStatement = fromExtentFragment as SqlSelectStatement;

                if (sqlSelectStatement != null)
                {
                    if (sqlSelectStatement.Select.IsEmpty)
                    {
                        List<Symbol> columns = this.AddDefaultColumns(sqlSelectStatement);

                        if (this.IsJoinExpression(input.Expression) || this.IsApplyExpression(input.Expression))
                        {
                            List<Symbol>    extents         = sqlSelectStatement.FromExtents;
                            JoinSymbol      newJoinSymbol   = new JoinSymbol(input.VariableName, input.VariableType, extents);

                            newJoinSymbol.IsNestedJoin  = true;
                            newJoinSymbol.ColumnList    = columns;
                            fromSymbol                  = newJoinSymbol;
                        }
                        else
                        {
                            // this is a copy of the code in CreateNewSelectStatement.

                            // if the oldStatement has a join as its input, ...
                            // clone the join symbol, so that we "reuse" the
                            // join symbol.  Normally, we create a new symbol - see the next block
                            // of code.
                            JoinSymbol oldJoinSymbol = sqlSelectStatement.FromExtents[0] as JoinSymbol;

                            if (oldJoinSymbol != null)
                            {
                                // Note: sqlSelectStatement.FromExtents will not do, since it might
                                // just be an alias of joinSymbol, and we want an actual JoinSymbol.
                                JoinSymbol newJoinSymbol = new JoinSymbol(input.VariableName, input.VariableType, oldJoinSymbol.ExtentList);

                                // This indicates that the sqlSelectStatement is a blocking scope
                                // i.e. it hides/renames extent columns
                                newJoinSymbol.IsNestedJoin          = true;
                                newJoinSymbol.ColumnList            = columns;
                                newJoinSymbol.FlattenedExtentList   = oldJoinSymbol.FlattenedExtentList;

                                fromSymbol = newJoinSymbol;
                            }
                        }
                    }

                    result.From.Append(" (");
                    result.From.Append(sqlSelectStatement);
                    result.From.Append(" )");
                }
                else if (input.Expression is DbScanExpression)
                {
                    result.From.Append(fromExtentFragment);
                }
                else // bracket it
                {
                    this.WrapNonQueryExtent(result, fromExtentFragment, input.Expression.ExpressionKind);
                }

                if (fromSymbol == null) // i.e. not a join symbol
                {
                    fromSymbol = new Symbol(input.VariableName, input.VariableType);
                }


                this.AddFromSymbol(result, input.VariableName, fromSymbol);

                result.AllJoinExtents.Add(fromSymbol);
            }
            else // result == fromExtentFragment.  The child extents have been merged into the parent's.
            {
                // we are adding extents to the current sql statement via flattening.
                // We are replacing the child's extents with a single Join symbol.
                // The child's extents are all those following the index fromSymbolStart.
                //
                List<Symbol> extents = new List<Symbol>();

                // We cannot call extents.AddRange, since the is no simple way to
                // get the range of symbols fromSymbolStart..result.FromExtents.Count
                // from result.FromExtents.
                // We copy these symbols to create the JoinSymbol later.
                for (int i = fromSymbolStart; i < result.FromExtents.Count; ++i)
                {
                    extents.Add(result.FromExtents[i]);
                }

                result.FromExtents.RemoveRange(fromSymbolStart, result.FromExtents.Count - fromSymbolStart);

                fromSymbol = new JoinSymbol(input.VariableName, input.VariableType, extents);
                result.FromExtents.Add(fromSymbol);
                
                // this Join Symbol does not have its own select statement, so we
                // do not set IsNestedJoin

                // We do not call AddFromSymbol(), since we do not want to add
                // "AS alias" to the FROM clause- it has been done when the extent was added earlier.
                this.symbolTable.Add(input.VariableName, fromSymbol);
            }
        }

        /// <summary>
        /// We assume that this is only called as a child of a Project.
        ///
        /// This replaces <see cref="Visit(NewInstanceExpression)"/>, since
        /// we do not allow NewInstanceExpression as a child of any node other than
        /// ProjectExpression.
        ///
        /// We write out the translation of each of the columns in the record.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>A <see cref="SqlBuilder"/></returns>
        private ISqlFragment VisitNewInstanceExpression(DbNewInstanceExpression e)
        {
            SqlBuilder  result      = new SqlBuilder();
            RowType     rowType     = e.ResultType.EdmType as RowType;
            string      separator   = "";

            if (null != rowType)
            {
                ReadOnlyMetadataCollection<EdmProperty> members = rowType.Properties;
                
                for(int i = 0; i < e.Arguments.Count; ++i)
                {
                    DbExpression argument = e.Arguments[i];

                    if (MetadataHelpers.IsRowType(argument.ResultType))
                    {
                        // We do not support nested records or other complex objects.
                        throw new NotSupportedException();
                    }

                    EdmProperty member = members[i];
                    
                    result.Append(separator);
                    result.AppendLine();
                    result.Append(argument.Accept(this));
                    result.Append(" AS ");
                    result.Append(QuoteIdentifier(member.Name));
                    
                    separator = ", ";
                }
            }
            else
            {
                //
                // Types other then RowType (such as UDTs for instance) are not supported.
                //
                throw new NotSupportedException();
            }

            return result;
        }

        private ISqlFragment VisitSetOpExpression(DbExpression left, DbExpression right, string separator)
        {
            SqlSelectStatement  leftSelectStatement     = this.VisitExpressionEnsureSqlStatement(left);
            SqlSelectStatement  rightSelectStatement    = this.VisitExpressionEnsureSqlStatement(right);
            SqlBuilder          setStatement            = new SqlBuilder();

            setStatement.Append(leftSelectStatement);
            setStatement.AppendLine();
            setStatement.Append(separator); // e.g. UNION ALL
            setStatement.AppendLine();
            setStatement.Append(rightSelectStatement);

            return setStatement;
        }


        #endregion

        #region � Helper methods for the ExpressionVisitor �

        /// <summary>
        /// <see cref="AddDefaultColumns"/>
        /// Add the column names from the referenced extent/join to the
        /// select statement.
        ///
        /// If the symbol is a JoinSymbol, we recursively visit all the extents,
        /// halting at real extents and JoinSymbols that have an associated SqlSelectStatement.
        ///
        /// The column names for a real extent can be derived from its type.
        /// The column names for a Join Select statement can be got from the
        /// list of columns that was created when the Join's select statement
        /// was created.
        ///
        /// We do the following for each column.
        /// <list type="number">
        /// <item>Add the SQL string for each column to the SELECT clause</item>
        /// <item>Add the column to the list of columns - so that it can
        /// become part of the "type" of a JoinSymbol</item>
        /// <item>Check if the column name collides with a previous column added
        /// to the same select statement.  Flag both the columns for renaming if true.</item>
        /// <item>Add the column to a name lookup dictionary for collision detection.</item>
        /// </list>
        /// </summary>
        /// <param name="selectStatement">The select statement that started off as SELECT *</param>
        /// <param name="symbol">The symbol containing the type information for
        /// the columns to be added.</param>
        /// <param name="columnList">Columns that have been added to the Select statement.
        /// This is created in <see cref="AddDefaultColumns"/>.</param>
        /// <param name="columnDictionary">A dictionary of the columns above.</param>
        /// <param name="separator">Comma or nothing, depending on whether the SELECT
        /// clause is empty.</param>
        private void AddColumns(
            SqlSelectStatement          selectStatement, 
            Symbol                      symbol,
            List<Symbol>                columnList, 
            Dictionary<string, Symbol>  columnDictionary, 
            ref string                  separator)
        {
            JoinSymbol joinSymbol = symbol as JoinSymbol;

            if (joinSymbol != null)
            {
                if (!joinSymbol.IsNestedJoin)
                {
                    // Recurse if the join symbol is a collection of flattened extents
                    foreach (Symbol sym in joinSymbol.ExtentList)
                    {
                        // if sym is ScalarType means we are at base case in the
                        // recursion and there are not columns to add, just skip
                        if ( MetadataHelpers.IsPrimitiveType(sym.Type) )
                        {
                            continue;
                        }

                        this.AddColumns(selectStatement, sym, columnList, columnDictionary, ref separator);
                    }
                }
                else
                {
                    foreach (Symbol joinColumn in joinSymbol.ColumnList)
                    {
                        // we write tableName.columnName
                        // rather than tableName.columnName as alias
                        // since the column name is unique (by the way we generate new column names)
                        //
                        // We use the symbols for both the table and the column,
                        // since they are subject to renaming.
                        selectStatement.Select.Append(separator);
                        selectStatement.Select.Append(symbol);
                        selectStatement.Select.Append(".");
                        selectStatement.Select.Append(joinColumn);

                        // check for name collisions.  If there is,
                        // flag both the colliding symbols.
                        if (columnDictionary.ContainsKey(joinColumn.Name))
                        {
                            columnDictionary[joinColumn.Name].NeedsRenaming = true; // the original symbol
                            joinColumn.NeedsRenaming = true; // the current symbol.
                        }
                        else
                        {
                            columnDictionary[joinColumn.Name] = joinColumn;
                        }

                        columnList.Add(joinColumn);

                        separator = ", ";
                    }
                }
            }
            else
            {
                // This is a non-join extent/select statement, and the CQT type has
                // the relevant column information.

                // The type could be a record type(e.g. Project(...),
                // or an entity type ( e.g. EntityExpression(...)
                // so, we check whether it is a structuralType.

                // Consider an expression of the form J(a, b=P(E))
                // The inner P(E) would have been translated to a SQL statement
                // We should not use the raw names from the type, but the equivalent
                // symbols (they are present in symbol.Columns) if they exist.
                //
                // We add the new columns to the symbol's columns if they do
                // not already exist.
                //

                foreach (EdmProperty property in MetadataHelpers.GetProperties(symbol.Type))
                {
                    string recordMemberName = property.Name;

                    // Since all renaming happens in the second phase
                    // we lose nothing by setting the next column name index to 0
                    // many times.
                    allColumnNames[recordMemberName] = 0;

                    // Create a new symbol/reuse existing symbol for the column
                    Symbol columnSymbol;

                    if (!symbol.Columns.TryGetValue(recordMemberName, out columnSymbol))
                    {
                        // we do not care about the types of columns, so we pass null
                        // when construction the symbol.
                        columnSymbol = new Symbol(recordMemberName, null);
                        symbol.Columns.Add(recordMemberName, columnSymbol);
                    }

                    selectStatement.Select.Append(separator);
                    selectStatement.Select.Append(symbol);
                    selectStatement.Select.Append(".");

                    // We use the actual name before the "AS", the new name goes
                    // after the AS.
                    selectStatement.Select.Append(QuoteIdentifier(recordMemberName));
                    selectStatement.Select.Append(" AS ");
                    selectStatement.Select.Append(columnSymbol);

                    // Check for column name collisions.
                    if (columnDictionary.ContainsKey(recordMemberName))
                    {
                        columnDictionary[recordMemberName].NeedsRenaming = true;
                        columnSymbol.NeedsRenaming = true;
                    }
                    else
                    {
                        columnDictionary[recordMemberName] = symbol.Columns[recordMemberName];
                    }

                    columnList.Add(columnSymbol);

                    separator = ", ";
                }
            }
        }

        /// <summary>
        /// Expands Select * to "select the_list_of_columns"
        /// If the columns are taken from an extent, they are written as
        /// {original_column_name AS Symbol(original_column)} to allow renaming.
        ///
        /// If the columns are taken from a Join, they are written as just
        /// {original_column_name}, since there cannot be a name collision.
        ///
        /// We concatenate the columns from each of the inputs to the select statement.
        /// Since the inputs may be joins that are flattened, we need to recurse.
        /// The inputs are inferred from the symbols in FromExtents.
        /// </summary>
        /// <param name="selectStatement"></param>
        /// <returns></returns>
        private List<Symbol> AddDefaultColumns(SqlSelectStatement selectStatement)
        {
            // This is the list of columns added in this select statement
            // This forms the "type" of the Select statement, if it has to
            // be expanded in another SELECT *
            List<Symbol> columnList = new List<Symbol>();

            // A lookup for the previous set of columns to aid column name
            // collision detection.
            Dictionary<string, Symbol> columnDictionary = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);

            string separator = "";

            // The Select should usually be empty before we are called,
            // but we do not mind if it is not.
            if (!selectStatement.Select.IsEmpty)
            {
                separator = ", ";
            }

            foreach (Symbol symbol in selectStatement.FromExtents)
            {
                this.AddColumns(selectStatement, symbol, columnList, columnDictionary, ref separator);
            }

            return columnList;
        }

        /// <summary>
        /// <see cref="AddFromSymbol(SqlSelectStatement, string, Symbol, bool)"/>
        /// </summary>
        /// <param name="selectStatement"></param>
        /// <param name="inputVarName"></param>
        /// <param name="fromSymbol"></param>
        private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol)
        {
            this.AddFromSymbol(selectStatement, inputVarName, fromSymbol, true);
        }

        /// <summary>
        /// This method is called after the input to a relational node is visited.
        /// <see cref="Visit(ProjectExpression)"/> and <see cref="ProcessJoinInputResult"/>
        /// There are 2 scenarios
        /// <list type="number">
        /// <item>The fromSymbol is new i.e. the select statement has just been
        /// created, or a join extent has been added.</item>
        /// <item>The fromSymbol is old i.e. we are reusing a select statement.</item>
        /// </list>
        ///
        /// If we are not reusing the select statement, we have to complete the
        /// FROM clause with the alias
        /// <code>
        /// -- if the input was an extent
        /// FROM = [SchemaName].[TableName]
        /// -- if the input was a Project
        /// FROM = (SELECT ... FROM ... WHERE ...)
        /// </code>
        ///
        /// These become
        /// <code>
        /// -- if the input was an extent
        /// FROM = [SchemaName].[TableName] AS alias
        /// -- if the input was a Project
        /// FROM = (SELECT ... FROM ... WHERE ...) AS alias
        /// </code>
        /// and look like valid FROM clauses.
        ///
        /// Finally, we have to add the alias to the global list of aliases used,
        /// and also to the current symbol table.
        /// </summary>
        /// <param name="selectStatement"></param>
        /// <param name="inputVarName">The alias to be used.</param>
        /// <param name="fromSymbol"></param>
        /// <param name="addToSymbolTable"></param>
        private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol, bool addToSymbolTable)
        {
            // the first check is true if this is a new statement
            // the second check is true if we are in a join - we do not
            // check if we are in a join context.
            // We do not want to add "AS alias" if it has been done already
            // e.g. when we are reusing the Sql statement.
            if (selectStatement.FromExtents.Count == 0 || fromSymbol != selectStatement.FromExtents[0])
            {
                selectStatement.FromExtents.Add(fromSymbol);
                selectStatement.From.Append(" AS ");
                selectStatement.From.Append(fromSymbol);

                // We have this inside the if statement, since
                // we only want to add extents that are actually used.
                this.allExtentNames[fromSymbol.Name] = 0;
            }

            if (addToSymbolTable)
            {
                this.symbolTable.Add(inputVarName, fromSymbol);
            }
        }

        /// <summary>
        /// Translates a list of SortClauses.
        /// Used in the translation of OrderBy 
        /// </summary>
        /// <param name="orderByClause">The SqlBuilder to which the sort keys should be appended</param>
        /// <param name="sortKeys"></param>
        private void AddSortKeys(SqlBuilder orderByClause, IList<DbSortClause> sortKeys)
        {
            string separator = "";

            foreach (DbSortClause sortClause in sortKeys)
            {
                orderByClause.Append(separator);
                orderByClause.Append(sortClause.Expression.Accept(this));

                // Bug 431021: COLLATE clause must precede ASC/DESC
                Debug.Assert(sortClause.Collation != null);

                if (!String.IsNullOrEmpty(sortClause.Collation))
                {
                    orderByClause.Append(" COLLATE ");
                    orderByClause.Append(sortClause.Collation);
                }

                orderByClause.Append(sortClause.Ascending ? " ASC" : " DESC");

                separator = ", ";
            }
        }

        /// <summary>
        /// This is called after a relational node's input has been visited, and the
        /// input's sql statement cannot be reused.  <see cref="Visit(ProjectExpression)"/>
        ///
        /// When the input's sql statement cannot be reused, we create a new sql
        /// statement, with the old one as the from clause of the new statement.
        ///
        /// The old statement must be completed i.e. if it has an empty select list,
        /// the list of columns must be projected out.
        ///
        /// If the old statement being completed has a join symbol as its from extent,
        /// the new statement must have a clone of the join symbol as its extent.
        /// We cannot reuse the old symbol, but the new select statement must behave
        /// as though it is working over the "join" record.
        /// </summary>
        /// <param name="oldStatement"></param>
        /// <param name="inputVarName"></param>
        /// <param name="inputVarType"></param>
        /// <param name="fromSymbol"></param>
        /// <returns>A new select statement, with the old one as the from clause.</returns>
        private SqlSelectStatement CreateNewSelectStatement(
            SqlSelectStatement  oldStatement,
            string              inputVarName, 
            TypeUsage           inputVarType, 
            out Symbol          fromSymbol)
        {
            fromSymbol = null;

            // Finalize the old statement
            if (oldStatement.Select.IsEmpty)
            {
                List<Symbol> columns = this.AddDefaultColumns(oldStatement);

                // Thid could not have been called from a join node.
                Debug.Assert(oldStatement.FromExtents.Count == 1);

                // if the oldStatement has a join as its input, ...
                // clone the join symbol, so that we "reuse" the
                // join symbol.  Normally, we create a new symbol - see the next block
                // of code.
                JoinSymbol oldJoinSymbol = oldStatement.FromExtents[0] as JoinSymbol;

                if (oldJoinSymbol != null)
                {
                    // Note: oldStatement.FromExtents will not do, since it might
                    // just be an alias of joinSymbol, and we want an actual JoinSymbol.
                    JoinSymbol newJoinSymbol = new JoinSymbol(inputVarName, inputVarType, oldJoinSymbol.ExtentList);

                    // This indicates that the oldStatement is a blocking scope
                    // i.e. it hides/renames extent columns
                    newJoinSymbol.IsNestedJoin = true;
                    newJoinSymbol.ColumnList = columns;
                    newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;

                    fromSymbol = newJoinSymbol;
                }
            }

            if (fromSymbol == null)
            {              
                // This is just a simple extent/SqlSelectStatement,
                // and we can get the column list from the type.
                fromSymbol = new Symbol(inputVarName, inputVarType);
            }

            // Observe that the following looks like the body of Visit(ExtentExpression).
            SqlSelectStatement selectStatement = new SqlSelectStatement();

            selectStatement.From.Append("( ");
            selectStatement.From.Append(oldStatement);
            selectStatement.From.AppendLine();
            selectStatement.From.Append(") ");

            return selectStatement;
        }

        /// <summary>
        /// Before we embed a string literal in a SQL string, we should
        /// convert all ' to '', and enclose the whole string in single quotes.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isUnicode"></param>
        /// <returns>The escaped sql string.</returns>
        private string EscapeSingleQuote(string s, bool isUnicode)
        {
            return (isUnicode ? "'" : "'") + s.Replace("'", "''") + "'";
        }

        /// <summary>
        /// Returns the sql primitive/native type name. 
        /// It will include size, precision or scale depending on type information present in the 
        /// type facets
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetSqlPrimitiveType(TypeUsage type)
        {
            PrimitiveType primitiveType = MetadataHelpers.GetEdmType<PrimitiveType>(type);

            string  typeName            = primitiveType.Name;
            bool    isUnicode           = true;
            bool    isFixedLength       = false;
            int     maxLength           = 0;
            string  length              = "max";
            bool    preserveSeconds     = true;
            byte    decimalPrecision    = 0;
            byte    decimalScale        = 0;

            switch (primitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Binary:
                    maxLength = MetadataHelpers.GetFacetValueOrDefault<int>(type, MetadataHelpers.MaxLengthFacetName);

                    if (maxLength == MetadataHelpers.BinaryMaxMaxLength)
                    {
                        length = "max";
                    }
                    else
                    {
                        length = maxLength.ToString(CultureInfo.InvariantCulture);
                    }

                    isFixedLength = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.FixedLengthFacetName);
                    typeName = (isFixedLength ? "binary(" : "varbinary(") + length + ")";
                    break;

                case PrimitiveTypeKind.String:
                    // Question: How do we handle ntext?
                    isUnicode       = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.UnicodeFacetName);
                    isFixedLength   = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.FixedLengthFacetName);
                    maxLength       = MetadataHelpers.GetFacetValueOrDefault<int>(type, MetadataHelpers.MaxLengthFacetName);
                    
                    length = maxLength.ToString(CultureInfo.InvariantCulture);

                    if (isFixedLength)
                    {
                        typeName = "char(" + length + ")";
                    }
                    else
                    {
                        typeName = "varchar(" + length + ")";
                    }
                    break;

                case PrimitiveTypeKind.DateTime:
                    preserveSeconds = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.PreserveSecondsFacetName);
                    typeName        = preserveSeconds ? "datetime" : "smalldatetime";
                    break;

                case PrimitiveTypeKind.Decimal:
                    decimalPrecision = MetadataHelpers.GetFacetValueOrDefault<byte>(type, MetadataHelpers.PrecisionFacetName);
                    Debug.Assert(decimalPrecision > 0, "decimal precision must be greater than zero");

                    decimalScale = MetadataHelpers.GetFacetValueOrDefault<byte>(type, MetadataHelpers.ScaleFacetName);
                    Debug.Assert(decimalPrecision >= decimalScale, "decimalPrecision must be greater or equal to decimalScale");
                    Debug.Assert(decimalPrecision <= 18, "decimalPrecision must be less than or equal to 18");

                    typeName = typeName + "(" + decimalPrecision + "," + decimalScale + ")";
                    break;

                default:
                    break;
            }

            return typeName;
        }

        /// <summary>
        /// Handles the expression represending LimitExpression.Limit and SkipExpression.Count.
        /// If it is a constant expression, it simply does to string thus avoiding casting it to the specific value
        /// (which would be done if <see cref="Visit(ConstantExpression)"/> is called)
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private ISqlFragment HandleCountExpression(DbExpression e)
        {
            ISqlFragment result;

            if (e.ExpressionKind == DbExpressionKind.Constant)
            {
                //For constant expression we should not cast the value, 
                // thus we don't go throught the default ConstantExpression handling
                SqlBuilder sqlBuilder = new SqlBuilder();

                sqlBuilder.Append(((DbConstantExpression)e).Value.ToString());
                
                result = sqlBuilder;
            }
            else
            {
                result = e.Accept(this);
            }

            return result;
        }

        /// <summary>
        /// This is used to determine if a particular expression is an Apply operation.
        /// This is only the case when the ExpressionKind is CrossApply or OuterApply.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsApplyExpression(DbExpression e)
        {
            return (DbExpressionKind.CrossApply == e.ExpressionKind || DbExpressionKind.OuterApply == e.ExpressionKind);
        }

        /// <summary>
        /// This is used to determine if a particular expression is a Join operation.
        /// This is true for CrossJoinExpression and JoinExpression, the
        /// latter of which may have one of several different ExpressionKinds.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsJoinExpression(DbExpression e)
        {
            return (DbExpressionKind.CrossJoin == e.ExpressionKind ||
                    DbExpressionKind.FullOuterJoin == e.ExpressionKind ||
                    DbExpressionKind.InnerJoin == e.ExpressionKind ||
                    DbExpressionKind.LeftOuterJoin == e.ExpressionKind);
        }

        /// <summary>
        /// This is used to determine if a calling expression needs to place
        /// round brackets around the translation of the expression e.
        ///
        /// Constants, parameters and properties do not require brackets,
        /// everything else does.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>true, if the expression needs brackets </returns>
        private bool IsComplexExpression(DbExpression e)
        {
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Constant:
                case DbExpressionKind.ParameterReference:
                case DbExpressionKind.Property:
                    return false;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Determine if the owner expression can add its unique sql to the input's
        /// SqlSelectStatement
        /// </summary>
        /// <param name="result">The SqlSelectStatement of the input to the relational node.</param>
        /// <param name="expressionKind">The kind of the expression node(not the input's)</param>
        /// <returns></returns>
        private bool IsCompatible(SqlSelectStatement result, DbExpressionKind expressionKind)
        {
            switch (expressionKind)
            {
                case DbExpressionKind.Distinct:
                    // The projection after distinct may not project all 
                    // columns used in the Order By
                    return result.Top == null && result.OrderBy.IsEmpty;

                case DbExpressionKind.Filter:
                    return result.Select.IsEmpty
                            && result.Where.IsEmpty
                            && result.GroupBy.IsEmpty
                            && result.Top == null;

                case DbExpressionKind.GroupBy:
                    return result.Select.IsEmpty
                            && result.GroupBy.IsEmpty
                            && result.OrderBy.IsEmpty
                            && result.Top == null;

                case DbExpressionKind.Limit:
                case DbExpressionKind.Element:
                    return result.Top == null;

                case DbExpressionKind.Project:
                    return result.Select.IsEmpty && result.GroupBy.IsEmpty;

                case DbExpressionKind.Skip:
                    return result.Select.IsEmpty
                            && result.GroupBy.IsEmpty
                            && result.OrderBy.IsEmpty
                            && !result.IsDistinct;

                case DbExpressionKind.Sort:
                    return result.Select.IsEmpty
                            && result.GroupBy.IsEmpty
                            && result.OrderBy.IsEmpty;

                default:
                    Debug.Assert(false);
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// We use the normal box quotes for SQL server.  We do not deal with ANSI quotes
        /// i.e. double quotes.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static string QuoteIdentifier(string name)
        {
            Debug.Assert(!String.IsNullOrEmpty(name));

            // We assume that the names are not quoted to begin with.
            return "\"" + name + "\"";
        }

        /// <summary>
        /// Simply calls <see cref="VisitExpressionEnsureSqlStatement(Expression, bool)"/>
        /// with addDefaultColumns set to true
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e)
        {
            return this.VisitExpressionEnsureSqlStatement(e, true);
        }

        /// <summary>
        /// This is called from <see cref="GenerateSql(DbQueryCommandTree)"/> and nodes which require a
        /// select statement as an argument e.g. <see cref="Visit(IsEmptyExpression)"/>,
        /// <see cref="Visit(UnionAllExpression)"/>.
        ///
        /// SqlGenerator needs its child to have a proper alias if the child is
        /// just an extent or a join.
        ///
        /// The normal relational nodes result in complete valid SQL statements.
        /// For the rest, we need to treat them as there was a dummy
        /// <code>
        /// -- originally {expression}
        /// -- change that to
        /// SELECT *
        /// FROM {expression} as c
        /// </code>
        /// 
        /// LimitExpression needs to start the statement but not add the default columns
        /// </summary>
        /// <param name="e"></param>
        /// <param name="addDefaultColumns"></param>
        /// <returns></returns>
        private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e, bool addDefaultColumns)
        {
            Debug.Assert(MetadataHelpers.IsCollectionType(e.ResultType));

            SqlSelectStatement result;

            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Project:
                case DbExpressionKind.Filter:
                case DbExpressionKind.GroupBy:
                case DbExpressionKind.Sort:
                    result = e.Accept(this) as SqlSelectStatement;
                    break;

                default:
                    Symbol fromSymbol;
                    string inputVarName = "c";  // any name will do - this is my random choice.
                    
                    this.symbolTable.EnterScope();

                    TypeUsage type = null;

                    switch (e.ExpressionKind)
                    {
                        case DbExpressionKind.Scan:
                        case DbExpressionKind.CrossJoin:
                        case DbExpressionKind.FullOuterJoin:
                        case DbExpressionKind.InnerJoin:
                        case DbExpressionKind.LeftOuterJoin:
                        case DbExpressionKind.CrossApply:
                        case DbExpressionKind.OuterApply:
                            type = MetadataHelpers.GetElementTypeUsage(e.ResultType);
                            break;

                        default:
                            Debug.Assert(MetadataHelpers.IsCollectionType(e.ResultType));
                            type = MetadataHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage;
                            break;
                    }

                    result = this.VisitInputExpression(e, inputVarName, type, out fromSymbol);
                    this.AddFromSymbol(result, inputVarName, fromSymbol);
                    this.symbolTable.ExitScope();
                    break;
            }

            if (addDefaultColumns && result.Select.IsEmpty)
            {
                this.AddDefaultColumns(result);
            }

            return result;
        }

        /// <summary>
        /// This method is called by <see cref="Visit(FilterExpression)"/> and
        /// <see cref="Visit(QuantifierExpression)"/>
        ///
        /// </summary>
        /// <param name="input"></param>
        /// <param name="predicate"></param>
        /// <param name="negatePredicate">This is passed from <see cref="Visit(QuantifierExpression)"/>
        /// in the All(...) case.</param>
        /// <returns></returns>
        private SqlSelectStatement VisitFilterExpression(DbExpressionBinding input, DbExpression predicate, bool negatePredicate)
        {
            Symbol              fromSymbol;
            SqlSelectStatement  result = VisitInputExpression(input.Expression, input.VariableName, input.VariableType, out fromSymbol);

            // Filter is compatible with OrderBy
            // but not with Project, another Filter or GroupBy
            if (!this.IsCompatible(result, DbExpressionKind.Filter))
            {
                result = this.CreateNewSelectStatement(result, input.VariableName, input.VariableType, out fromSymbol);
            }

            this.selectStatementStack.Push(result);
            this.symbolTable.EnterScope();

            this.AddFromSymbol(result, input.VariableName, fromSymbol);

            if (negatePredicate)
            {
                result.Where.Append("NOT (");
            }
            result.Where.Append(predicate.Accept(this));

            if (negatePredicate)
            {
                result.Where.Append(")");
            }

            this.symbolTable.ExitScope();
            this.selectStatementStack.Pop();

            return result;
        }

        /// <summary>
        /// If the sql fragment for an input expression is not a SqlSelect statement
        /// or other acceptable form (e.g. an extent as a SqlBuilder), we need
        /// to wrap it in a form acceptable in a FROM clause.  These are
        /// primarily the
        /// <list type="bullet">
        /// <item>The set operation expressions - union all, intersect, except</item>
        /// <item>TVFs, which are conceptually similar to tables</item>
        /// </list>
        /// </summary>
        /// <param name="result"></param>
        /// <param name="sqlFragment"></param>
        /// <param name="expressionKind"></param>
        private void WrapNonQueryExtent(SqlSelectStatement result, ISqlFragment sqlFragment, DbExpressionKind expressionKind)
        {
            switch (expressionKind)
            {
                case DbExpressionKind.Function:
                    // TVF
                    result.From.Append(sqlFragment);
                    break;

                default:
                    result.From.Append(" (");
                    result.From.Append(sqlFragment);
                    result.From.Append(")");
                    break;
            }
        }

        /// <summary>
        /// Is this a builtin function (ie) does it have the builtinAttribute specified?
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        private static bool IsBuiltinFunction(EdmFunction function)
        {
            return MetadataHelpers.TryGetValueForMetadataProperty<bool>(function, "BuiltInAttribute");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="function"></param>
        /// <param name="result"></param>
        private void WriteFunctionName(SqlBuilder result, EdmFunction function)
        {
            string storeFunctionName = MetadataHelpers.TryGetValueForMetadataProperty<string>(function, "StoreFunctionNameAttribute");

            if (string.IsNullOrEmpty(storeFunctionName))
            {
                storeFunctionName = function.Name;
            }

            // If the function is a builtin (ie) the BuiltIn attribute has been
            // specified, then, the function name should not be quoted; additionally,
            // no namespace should be used.
            if (IsBuiltinFunction(function))
            {
                result.Append(storeFunctionName);
            }
            else
            {
                // Should we actually support this?
                result.Append(QuoteIdentifier(function.NamespaceName));
                result.Append(".");
                result.Append(QuoteIdentifier(storeFunctionName));
            }           
        }

        private static string ByteArrayToBinaryString(Byte[] binaryArray)
        {
            StringBuilder sb = new StringBuilder( binaryArray.Length * 2 );
            
            for (int i = 0 ; i < binaryArray.Length ; i++)
            {
                sb.Append(HexDigits[(binaryArray[i]&0xF0) >>4]).Append(HexDigits[binaryArray[i]&0x0F]);
            }

            return sb.ToString();
        }

        #endregion
    }
}

#endif