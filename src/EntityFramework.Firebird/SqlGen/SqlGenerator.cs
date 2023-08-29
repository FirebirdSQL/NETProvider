/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using FirebirdSql.Data.Common;

namespace EntityFramework.Firebird.SqlGen;

internal sealed class SqlGenerator : DbExpressionVisitor<ISqlFragment>
{
	#region Visitor parameter stacks
	/// <summary>
	/// Every relational node has to pass its SELECT statement to its children
	/// This allows them (DbVariableReferenceExpression eventually) to update the list of
	/// outer extents (free variables) used by this select statement.
	/// </summary>
	Stack<SqlSelectStatement> _selectStatementStack;

	/// <summary>
	/// The top of the stack
	/// </summary>
	private SqlSelectStatement CurrentSelectStatement
	{
		// There is always something on the stack, so we can always Peek.
		get { return _selectStatementStack.Peek(); }
	}

	/// <summary>
	/// Nested joins and extents need to know whether they should create
	/// a new Select statement, or reuse the parent's.  This flag
	/// indicates whether the parent is a join or not.
	/// </summary>
	Stack<bool> _isParentAJoinStack;

	/// <summary>
	/// The top of the stack
	/// </summary>
	private bool IsParentAJoin
	{
		// There might be no entry on the stack if a Join node has never
		// been seen, so we return false in that case.
		get { return _isParentAJoinStack.Count == 0 ? false : _isParentAJoinStack.Peek(); }
	}

	#endregion

	#region Global lists and state
	Dictionary<string, int> _allExtentNames;
	internal Dictionary<string, int> AllExtentNames
	{
		get { return _allExtentNames; }
	}

	// For each column name, we store the last integer suffix that
	// was added to produce a unique column name.  This speeds up
	// the creation of the next unique name for this column name.
	Dictionary<string, int> _allColumnNames;
	internal Dictionary<string, int> AllColumnNames
	{
		get { return _allColumnNames; }
	}

	SymbolTable _symbolTable = new SymbolTable();

	/// <summary>
	/// VariableReferenceExpressions are allowed only as children of DbPropertyExpression
	/// or MethodExpression.  The cheapest way to ensure this is to set the following
	/// property in DbVariableReferenceExpression and reset it in the allowed parent expressions.
	/// </summary>
	bool _isVarRefSingle = false;

	bool _shouldHandleBoolComparison = true;
	bool _shouldCastParameter = true;

	Dictionary<string, string> _shortenedNames = new Dictionary<string, string>();

	#endregion

	#region Statics
	static private readonly Dictionary<string, FunctionHandler> _builtInFunctionHandlers = InitializeBuiltInFunctionHandlers();
	static private readonly Dictionary<string, FunctionHandler> _canonicalFunctionHandlers = InitializeCanonicalFunctionHandlers();
	static private readonly Dictionary<string, string> _functionNameToOperatorDictionary = InitializeFunctionNameToOperatorDictionary();

	private delegate ISqlFragment FunctionHandler(SqlGenerator sqlgen, DbFunctionExpression functionExpr);

	/// <summary>
	/// All special built-in functions and their handlers
	/// </summary>
	/// <returns></returns>
	private static Dictionary<string, FunctionHandler> InitializeBuiltInFunctionHandlers()
	{
		var functionHandlers = new Dictionary<string, FunctionHandler>(0, StringComparer.Ordinal);
		return functionHandlers;
	}

	/// <summary>
	/// All special non-aggregate canonical functions and their handlers
	/// </summary>
	/// <returns></returns>
	private static Dictionary<string, FunctionHandler> InitializeCanonicalFunctionHandlers()
	{
		var functionHandlers = new Dictionary<string, FunctionHandler>(StringComparer.Ordinal);

		#region Other Canonical Functions
		functionHandlers.Add("NewGuid", HandleCanonicalFunctionNewGuid);
		#endregion

		#region Math Canonical Functions
		functionHandlers.Add("Abs", HandleCanonicalFunctionAbs);
		functionHandlers.Add("Ceiling", HandleCanonicalFunctionCeiling);
		functionHandlers.Add("Floor", HandleCanonicalFunctionFloor);
		functionHandlers.Add("Power", HandleCanonicalFunctionPower);
		functionHandlers.Add("Round", HandleCanonicalFunctionRound);
		functionHandlers.Add("Truncate", HandleCanonicalFunctionTruncate);
		#endregion

		#region String Canonical Functions
		functionHandlers.Add("Concat", HandleCanonicalConcatFunction);
		functionHandlers.Add("Contains", HandleCanonicalContainsFunction);
		functionHandlers.Add("EndsWith", HandleCanonicalEndsWithFunction);
		functionHandlers.Add("IndexOf", HandleCanonicalFunctionIndexOf);
		functionHandlers.Add("Length", HandleCanonicalFunctionLength);
		functionHandlers.Add("ToLower", HandleCanonicalFunctionToLower);
		functionHandlers.Add("ToUpper", HandleCanonicalFunctionToUpper);
		functionHandlers.Add("Trim", HandleCanonicalFunctionTrim);
		functionHandlers.Add("LTrim", HandleCanonicalFunctionLTrim);
		functionHandlers.Add("RTrim", HandleCanonicalFunctionRTrim);
		functionHandlers.Add("Left", HandleCanonicalFunctionLeft);
		functionHandlers.Add("Right", HandleCanonicalFunctionRight);
		functionHandlers.Add("Reverse", HandleCanonicalFunctionReverse);
		functionHandlers.Add("Replace", HandleCanonicalFunctionReplace);
		functionHandlers.Add("StartsWith", HandleCanonicalStartsWithFunction);
		functionHandlers.Add("Substring", HandleCanonicalFunctionSubstring);
		#endregion

		#region Date and Time Canonical Functions
		functionHandlers.Add("AddNanoseconds", (sqlgen, e) => HandleCanonicalFunctionDateTimeAdd(sqlgen, e, null)); // not supported
		functionHandlers.Add("AddMicroseconds", (sqlgen, e) => HandleCanonicalFunctionDateTimeAdd(sqlgen, e, null)); // not supported
		functionHandlers.Add("AddMilliseconds", (sqlgen, e) => HandleCanonicalFunctionDateTimeAdd(sqlgen, e, "MILLISECOND"));
		functionHandlers.Add("AddSeconds", (sqlgen, e) => HandleCanonicalFunctionDateTimeAdd(sqlgen, e, "SECOND"));
		functionHandlers.Add("AddMinutes", (sqlgen, e) => HandleCanonicalFunctionDateTimeAdd(sqlgen, e, "MINUTE"));
		functionHandlers.Add("AddHours", (sqlgen, e) => HandleCanonicalFunctionDateTimeAdd(sqlgen, e, "HOUR"));
		functionHandlers.Add("AddDays", (sqlgen, e) => HandleCanonicalFunctionDateTimeAdd(sqlgen, e, "DAY"));
		functionHandlers.Add("AddMonths", (sqlgen, e) => HandleCanonicalFunctionDateTimeAdd(sqlgen, e, "MONTH"));
		functionHandlers.Add("AddYears", (sqlgen, e) => HandleCanonicalFunctionDateTimeAdd(sqlgen, e, "YEAR"));
		functionHandlers.Add("CreateDateTime", HandleCanonicalFunctionCreateDateTime);
		functionHandlers.Add("CreateDateTimeOffset", HandleCanonicalFunctionCreateDateTimeOffset); // not supported
		functionHandlers.Add("CreateTime", HandleCanonicalFunctionCreateTime);
		functionHandlers.Add("CurrentDateTime", HandleCanonicalFunctionCurrentDateTime);
		functionHandlers.Add("CurrentDateTimeOffset", HandleCanonicalFunctionCurrentDateTimeOffset); // not supported
		functionHandlers.Add("CurrentUtcDateTime", HandleCanonicalFunctionCurrentUtcDateTime); // not supported
		functionHandlers.Add("Day", (sqlgen, e) => HandleCanonicalFunctionExtract(sqlgen, e, "DAY"));
		functionHandlers.Add("DayOfYear", (sqlgen, e) => HandleCanonicalFunctionExtract(sqlgen, e, "YEARDAY"));
		functionHandlers.Add("DiffNanoseconds", (sqlgen, e) => HandleCanonicalFunctionDateTimeDiff(sqlgen, e, null)); // not supported
		functionHandlers.Add("DiffMicroseconds", (sqlgen, e) => HandleCanonicalFunctionDateTimeDiff(sqlgen, e, null)); // not supported
		functionHandlers.Add("DiffMilliseconds", (sqlgen, e) => HandleCanonicalFunctionDateTimeDiff(sqlgen, e, "MILLISECOND"));
		functionHandlers.Add("DiffSeconds", (sqlgen, e) => HandleCanonicalFunctionDateTimeDiff(sqlgen, e, "SECOND"));
		functionHandlers.Add("DiffMinutes", (sqlgen, e) => HandleCanonicalFunctionDateTimeDiff(sqlgen, e, "MINUTE"));
		functionHandlers.Add("DiffHours", (sqlgen, e) => HandleCanonicalFunctionDateTimeDiff(sqlgen, e, "HOUR"));
		functionHandlers.Add("DiffDays", (sqlgen, e) => HandleCanonicalFunctionDateTimeDiff(sqlgen, e, "DAY"));
		functionHandlers.Add("DiffMonths", (sqlgen, e) => HandleCanonicalFunctionDateTimeDiff(sqlgen, e, "MONTH"));
		functionHandlers.Add("DiffYears", (sqlgen, e) => HandleCanonicalFunctionDateTimeDiff(sqlgen, e, "YEAR"));
		functionHandlers.Add("GetTotalOffsetMinutes", HandleCanonicalFunctionGetTotalOffsetMinutes); // not supported
		functionHandlers.Add("Hour", (sqlgen, e) => HandleCanonicalFunctionExtract(sqlgen, e, "HOUR"));
		functionHandlers.Add("Millisecond", (sqlgen, e) => HandleCanonicalFunctionExtract(sqlgen, e, "MILLISECOND"));
		functionHandlers.Add("Minute", (sqlgen, e) => HandleCanonicalFunctionExtract(sqlgen, e, "MINUTE"));
		functionHandlers.Add("Month", (sqlgen, e) => HandleCanonicalFunctionExtract(sqlgen, e, "MONTH"));
		functionHandlers.Add("Second", (sqlgen, e) => HandleCanonicalFunctionExtract(sqlgen, e, "SECOND"));
		functionHandlers.Add("TruncateTime", HandleCanonicalFunctionTruncateTime);
		functionHandlers.Add("Year", (sqlgen, e) => HandleCanonicalFunctionExtract(sqlgen, e, "YEAR"));
		#endregion

		#region Bitwise Canonical Functions
		functionHandlers.Add("BitwiseAnd", HandleCanonicalFunctionBitwiseAnd);
		functionHandlers.Add("BitwiseNot", HandleCanonicalFunctionBitwiseNot); // not supported
		functionHandlers.Add("BitwiseOr", HandleCanonicalFunctionBitwiseOr);
		functionHandlers.Add("BitwiseXor", HandleCanonicalFunctionBitwiseXor);
		#endregion

		return functionHandlers;
	}

	/// <summary>
	/// Initializes the mapping from functions to T-SQL operators
	/// for all functions that translate to T-SQL operators
	/// </summary>
	/// <returns></returns>
	private static Dictionary<string, string> InitializeFunctionNameToOperatorDictionary()
	{
		return new Dictionary<string, string>(StringComparer.Ordinal)
			{
				{ nameof(string.Concat), "||" },
				{ nameof(string.Contains), "CONTAINING" },
				{ nameof(string.StartsWith), "STARTING WITH" },
			};
	}

	#endregion

	#region Constructor
	/// <summary>
	/// Basic constructor.
	/// </summary>
	internal SqlGenerator()
	{
	}
	#endregion

	#region Entry points
	/// <summary>
	/// General purpose static function that can be called from System.Data assembly
	/// </summary>
	/// <param name="sqlVersion">Server version</param>
	/// <param name="tree">command tree</param>
	/// <param name="parameters">Parameters to add to the command tree corresponding
	/// to constants in the command tree. Used only in ModificationCommandTrees.</param>
	/// <returns>The string representing the SQL to be executed.</returns>
	internal static string GenerateSql(DbCommandTree tree, out List<DbParameter> parameters, out CommandType commandType)
	{
		commandType = CommandType.Text;

		//Handle Query
		if (tree is DbQueryCommandTree queryCommandTree)
		{
			var sqlGen = new SqlGenerator();
			parameters = null;
			return sqlGen.GenerateSql((DbQueryCommandTree)tree);
		}

		//Handle Function
		if (tree is DbFunctionCommandTree DbFunctionCommandTree)
		{
			var sqlGen = new SqlGenerator();
			parameters = null;

			var sql = sqlGen.GenerateFunctionSql(DbFunctionCommandTree, out commandType);

			return sql;
		}

		//Handle Insert
		if (tree is DbInsertCommandTree insertCommandTree)
		{
			return DmlSqlGenerator.GenerateInsertSql(insertCommandTree, out parameters);
		}

		//Handle Delete
		if (tree is DbDeleteCommandTree deleteCommandTree)
		{
			return DmlSqlGenerator.GenerateDeleteSql(deleteCommandTree, out parameters);
		}

		//Handle Update
		if (tree is DbUpdateCommandTree updateCommandTree)
		{
			return DmlSqlGenerator.GenerateUpdateSql(updateCommandTree, out parameters);
		}

		throw new NotSupportedException("Unrecognized command tree type");
	}
	#endregion

	#region Driver Methods
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
		_selectStatementStack = new Stack<SqlSelectStatement>();
		_isParentAJoinStack = new Stack<bool>();

		_allExtentNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		_allColumnNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		// Literals will not be converted to parameters.

		ISqlFragment result;
		if (MetadataHelpers.IsCollectionType(tree.Query.ResultType))
		{
			var sqlStatement = VisitExpressionEnsureSqlStatement(tree.Query);
			Debug.Assert(sqlStatement != null, "The outer most sql statment is null");
			sqlStatement.IsTopMost = true;
			result = sqlStatement;

		}
		else
		{
			var sqlBuilder = new SqlBuilder();
			sqlBuilder.Append("SELECT ");
			sqlBuilder.Append(tree.Query.Accept(this));

			result = sqlBuilder;
		}

		if (_isVarRefSingle)
		{
			throw new NotSupportedException();
			// A DbVariableReferenceExpression has to be a child of DbPropertyExpression or MethodExpression
		}

		// Check that the parameter stacks are not leaking.
		Debug.Assert(_selectStatementStack.Count == 0);
		Debug.Assert(_isParentAJoinStack.Count == 0);

		return WriteSql(result);
	}

	/// <summary>
	/// Translate a function command tree to a SQL string.
	/// </summary>
	private string GenerateFunctionSql(DbFunctionCommandTree tree, out CommandType commandType)
	{
		var function = tree.EdmFunction;

		// We expect function to always have these properties
		var userCommandText = (string)function.MetadataProperties["CommandTextAttribute"].Value;
		/// No schema in FB
		//string userSchemaName = (string)function.MetadataProperties["Schema"].Value;
		var userFuncName = (string)function.MetadataProperties["StoreFunctionNameAttribute"].Value;

		if (string.IsNullOrEmpty(userCommandText))
		{
			// build a quoted description of the function
			commandType = CommandType.StoredProcedure;

			// if the schema name is not explicitly given, it is assumed to be the metadata namespace
			/// No schema in FB
			//string schemaName = string.IsNullOrEmpty(userSchemaName) ?
			//    function.NamespaceName : userSchemaName;

			// if the function store name is not explicitly given, it is assumed to be the metadata name
			var functionName = string.IsNullOrEmpty(userFuncName) ?
				function.Name : userFuncName;

			// quote elements of function text
			/// No schema in FB
			//string quotedSchemaName = QuoteIdentifier(schemaName);
			var quotedFunctionName = QuoteIdentifier(functionName);

			// separator
			/// No schema in FB
			//const string schemaSeparator = ".";
			// concatenate elements of function text

			/// No schema in FB
			var quotedFunctionText = /*quotedSchemaName + schemaSeparator + */quotedFunctionName;

			return quotedFunctionText;
		}
		else
		{
			// if the user has specified the command text, pass it through verbatim and choose CommandType.Text
			commandType = CommandType.Text;
			return userCommandText;
		}
	}

	/// <summary>
	/// Convert the SQL fragments to a string.
	/// We have to setup the Stream for writing.
	/// </summary>
	/// <param name="sqlStatement"></param>
	/// <returns>A string representing the SQL to be executed.</returns>
	string WriteSql(ISqlFragment sqlStatement)
	{
		var builder = new StringBuilder(1024);
		using (var writer = new SqlWriter(builder))
		{
			WriteSql(writer, sqlStatement);
		}

		return builder.ToString();
	}

	internal SqlWriter WriteSql(SqlWriter writer, ISqlFragment sqlStatement)
	{
		sqlStatement.WriteSql(writer, this);
		return writer;
	}
	#endregion

	#region DbExpressionVisitor Members

	/// <summary>
	/// Translate(left) AND Translate(right)
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/>.</returns>
	public override ISqlFragment Visit(DbAndExpression e)
	{
		return VisitBinaryExpression(" AND ", e.Left, e.Right);
	}

	/// <summary>
	/// An apply is just like a join, so it shares the common join processing
	/// in <see cref="VisitJoinExpression"/>
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlSelectStatement"/>.</returns>
	public override ISqlFragment Visit(DbApplyExpression e)
	{
		string joinString;
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

		throw new NotSupportedException($"{joinString} statement is not supported in Firebird.");
		// The join condition does not exist in this case, so we use null.
		// We do not have a on clause, so we use JoinType.CrossJoin.
		//return VisitJoinExpression(inputs, DbExpressionKind.CrossJoin, joinString, null);
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
				result = VisitBinaryExpression(" / ", e.Arguments[0], e.Arguments[1]);
				break;
			case DbExpressionKind.Minus:
				result = VisitBinaryExpression(" - ", e.Arguments[0], e.Arguments[1]);
				break;
			case DbExpressionKind.Modulo:
				//result = VisitBinaryExpression(" % ", e.Arguments[0], e.Arguments[1]);
				result = new SqlBuilder();
				result.Append(" MOD(");
				result.Append(e.Arguments[0].Accept(this));
				result.Append(", ");
				result.Append(e.Arguments[1].Accept(this));
				result.Append(")");
				break;
			case DbExpressionKind.Multiply:
				result = VisitBinaryExpression(" * ", e.Arguments[0], e.Arguments[1]);
				break;
			case DbExpressionKind.Plus:
				result = VisitBinaryExpression(" + ", e.Arguments[0], e.Arguments[1]);
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
		var result = new SqlBuilder();

		Debug.Assert(e.When.Count == e.Then.Count);

		result.Append("CASE");
		for (var i = 0; i < e.When.Count; ++i)
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
		var result = new SqlBuilder();
		var sqlPrimitiveType = GetSqlPrimitiveType(e.ResultType);

		switch (sqlPrimitiveType.ToUpperInvariant())
		{
			default:
				result.Append("CAST(");
				result.Append(e.Argument.Accept(this));
				result.Append(" AS ");
				result.Append(sqlPrimitiveType);
				result.Append(")");
				break;
		}

		return result;
	}

	/// <summary>
	/// The parser generates Not(Equals(...)) for &lt;&gt;.
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/>.</returns>
	public override ISqlFragment Visit(DbComparisonExpression e)
	{
		switch (e.ExpressionKind)
		{
			case DbExpressionKind.Equals:
				return VisitBinaryExpression(" = ", e.Left, e.Right);
			case DbExpressionKind.LessThan:
				return VisitBinaryExpression(" < ", e.Left, e.Right);
			case DbExpressionKind.LessThanOrEquals:
				return VisitBinaryExpression(" <= ", e.Left, e.Right);
			case DbExpressionKind.GreaterThan:
				return VisitBinaryExpression(" > ", e.Left, e.Right);
			case DbExpressionKind.GreaterThanOrEquals:
				return VisitBinaryExpression(" >= ", e.Left, e.Right);
			// The parser does not generate the expression kind below.
			case DbExpressionKind.NotEquals:
				return VisitBinaryExpression(" <> ", e.Left, e.Right);

			default:
				Debug.Assert(false);  // The constructor should have prevented this
				throw new InvalidOperationException(string.Empty);
		}
	}

	/// <summary>
	/// Constants will be send to the store as part of the generated SQL, not as parameters.
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/>.  Strings are wrapped in single
	/// quotes and escaped.  Numbers are written literally.</returns>
	public override ISqlFragment Visit(DbConstantExpression e)
	{
		var result = new SqlBuilder();

		if (MetadataHelpers.TryGetPrimitiveTypeKind(e.ResultType, out var typeKind))
		{
			switch (typeKind)
			{
				case PrimitiveTypeKind.Boolean:
					result.Append(FormatBoolean((bool)e.Value));
					break;

				case PrimitiveTypeKind.Int16:
					result.Append("CAST(");
					result.Append(e.Value.ToString());
					result.Append(" AS ");
					result.Append(GetSqlPrimitiveType(e.ResultType));
					result.Append(")");
					break;

				case PrimitiveTypeKind.Int32:
					// default for integral values.
					result.Append(e.Value.ToString());
					break;

				case PrimitiveTypeKind.Int64:
					result.Append("CAST(");
					result.Append(e.Value.ToString());
					result.Append(" AS ");
					result.Append(GetSqlPrimitiveType(e.ResultType));
					result.Append(")");
					break;

				case PrimitiveTypeKind.Double:
					result.Append("CAST(");
					result.Append(((Double)e.Value).ToString(CultureInfo.InvariantCulture));
					result.Append(" AS ");
					result.Append(GetSqlPrimitiveType(e.ResultType));
					result.Append(")");
					break;

				case PrimitiveTypeKind.Single:
					result.Append("CAST(");
					result.Append(((Single)e.Value).ToString(CultureInfo.InvariantCulture));
					result.Append(" AS ");
					result.Append(GetSqlPrimitiveType(e.ResultType));
					result.Append(")");
					break;

				case PrimitiveTypeKind.Decimal:
					var sqlPrimitiveType = GetSqlPrimitiveType(e.ResultType);
					var strDecimal = ((Decimal)e.Value).ToString(CultureInfo.InvariantCulture);

					var pointPosition = strDecimal.IndexOf('.');

					var precision = 9;
					// there's always the max value in manifest
					if (MetadataHelpers.TryGetTypeFacetDescriptionByName(e.ResultType.EdmType, MetadataHelpers.PrecisionFacetName, out var precisionFacetDescription))
					{
						if (precisionFacetDescription.DefaultValue != null)
							precision = (int)precisionFacetDescription.DefaultValue;
					}

					var maxScale = (pointPosition != -1 ? precision - pointPosition + 1 : 0);

					result.Append("CAST(");
					result.Append(strDecimal);
					result.Append(" AS ");
					result.Append(sqlPrimitiveType.Substring(0, sqlPrimitiveType.IndexOf('(')));
					result.Append("(");
					result.Append(precision.ToString(CultureInfo.InvariantCulture));
					result.Append(",");
					result.Append(maxScale.ToString(CultureInfo.InvariantCulture));
					result.Append("))");
					break;

				case PrimitiveTypeKind.Binary:
					result.Append(FormatBinary((byte[])e.Value));
					break;

				case PrimitiveTypeKind.String:
					var isUnicode = MetadataHelpers.GetFacetValueOrDefault<bool>(e.ResultType, MetadataHelpers.UnicodeFacetName, true);
					// constant is always considered Unicode
					isUnicode = true;
					var length = MetadataHelpers.GetFacetValueOrDefault<int?>(e.ResultType, MetadataHelpers.MaxLengthFacetName, null)
						?? (isUnicode ? FbProviderManifest.UnicodeVarcharMaxSize : FbProviderManifest.AsciiVarcharMaxSize);
					result.Append(FormatString((string)e.Value, isUnicode, length));
					break;

				case PrimitiveTypeKind.DateTime:
					result.Append(FormatDateTime((DateTime)e.Value));
					break;
				case PrimitiveTypeKind.Time:
					result.Append(FormatTime((DateTime)e.Value));
					break;

				case PrimitiveTypeKind.Guid:
					result.Append(FormatGuid((Guid)e.Value));
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
	/// <see cref="DbDerefExpression"/> is illegal at this stage
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
		var result = VisitExpressionEnsureSqlStatement(e.Argument);

		if (!IsCompatible(result, e.ExpressionKind))
		{
			var inputType = MetadataHelpers.GetElementTypeUsage(e.Argument.ResultType);
			result = CreateNewSelectStatement(result, "DISTINCT", inputType, out var fromSymbol);
			AddFromSymbol(result, "DISTINCT", fromSymbol, false);
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
		var result = new SqlBuilder();
		result.Append("(");
		result.Append(VisitExpressionEnsureSqlStatement(e.Argument));
		result.Append(")");

		return result;
	}

	/// <summary>
	/// <see cref="Visit(DbUnionAllExpression)"/>
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	public override ISqlFragment Visit(DbExceptExpression e)
	{
		throw new NotSupportedException("The EXCEPT statement is not supported in Firebird.");
		//return VisitSetOpExpression(e.Left, e.Right, "EXCEPT");
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
		var target = e.Target;

		if (IsParentAJoin)
		{
			var result = new SqlBuilder();
			result.Append(GetTargetSql(target));

			return result;
		}
		else
		{
			var result = new SqlSelectStatement();
			result.From.Append(GetTargetSql(target));

			return result;
		}
	}


	/// <summary>
	/// Gets escaped SQL identifier describing this entity set.
	/// </summary>
	/// <returns></returns>
	internal static string GetTargetSql(EntitySetBase entitySetBase)
	{
		// construct escaped SQL referencing entity set
		var builder = new StringBuilder();
		var definingQuery = MetadataHelpers.TryGetValueForMetadataProperty<string>(entitySetBase, "DefiningQuery");
		if (!string.IsNullOrEmpty(definingQuery))
		{
			builder.Append("(");
			builder.Append(definingQuery);
			builder.Append(")");
		}
		else
		{
			/// No schema in FB
			//string schemaName = MetadataHelpers.TryGetValueForMetadataProperty<string>(entitySetBase, "Schema");
			//if (!string.IsNullOrEmpty(schemaName))
			//{
			//    builder.Append(QuoteIdentifier(schemaName));
			//    builder.Append(".");
			//}
			//else
			//{
			//    builder.Append(QuoteIdentifier(entitySetBase.EntityContainer.Name));
			//    builder.Append(".");
			//}

			builder.Append(QuoteIdentifier(MetadataHelpers.GetTableName(entitySetBase)));
		}
		return builder.ToString();
	}

	/// <summary>
	/// The bodies of <see cref="Visit(DbFilterExpression)"/>, <see cref="Visit(DbGroupByExpression)"/>,
	/// <see cref="Visit(DbProjectExpression)"/>, <see cref="Visit(DbSortExpression)"/> are similar.
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
	/// The functions supported are:
	/// <list type="number">
	/// <item>Canonical Functions - We recognize these by their dataspace, it is DataSpace.CSpace</item>
	/// <item>Store Functions - We recognize these by the BuiltInAttribute and not being Canonical</item>
	/// <item>User-defined Functions - All the rest except for Lambda functions</item>
	/// </list>
	/// We handle Canonical and Store functions the same way: If they are in the list of functions
	/// that need special handling, we invoke the appropriate handler, otherwise we translate them to
	/// FunctionName(arg1, arg2, ..., argn).
	/// We translate user-defined functions to NamespaceName.FunctionName(arg1, arg2, ..., argn).
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/></returns>
	public override ISqlFragment Visit(DbFunctionExpression e)
	{
		//
		// check if function requires special case processing, if so, delegates to it
		//
		if (IsSpecialBuiltInFunction(e))
		{
			return HandleSpecialBuiltInFunction(e);
		}

		if (IsSpecialCanonicalFunction(e))
		{
			return HandleSpecialCanonicalFunction(e);
		}

		return HandleFunctionDefault(e);
	}


	/// <summary>
	/// <see cref="DbEntityRefExpression"/> is illegal at this stage
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	public override ISqlFragment Visit(DbEntityRefExpression e)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// <see cref="DbRefKeyExpression"/> is illegal at this stage
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	public override ISqlFragment Visit(DbRefKeyExpression e)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// <see cref="Visit(DbFilterExpression)"/> for general details.
	/// We modify both the GroupBy and the Select fields of the SqlSelectStatement.
	/// GroupBy gets just the keys without aliases,
	/// and Select gets the keys and the aggregates with aliases.
	///
	/// Whenever there exists at least one aggregate with an argument that is not is not a simple
	/// <see cref="DbPropertyExpression"/>  over <see cref="DbVariableReferenceExpression"/>,
	/// we create a nested query in which we alias the arguments to the aggregates.
	/// That is due to the following two limitations of Sql Server:
	/// <list type="number">
	/// <item>If an expression being aggregated contains an outer reference, then that outer
	/// reference must be the only column referenced in the expression </item>
	/// <item>Sql Server cannot perform an aggregate function on an expression containing
	/// an aggregate or a subquery. </item>
	/// </list>
	///
	/// The default translation, without inner query is:
	///
	///     SELECT
	///         kexp1 AS key1, kexp2 AS key2,... kexpn AS keyn,
	///         aggf1(aexpr1) AS agg1, .. aggfn(aexprn) AS aggn
	///     FROM input AS a
	///     GROUP BY kexp1, kexp2, .. kexpn
	///
	/// When we inject an innner query, the equivalent translation is:
	///
	///     SELECT
	///         key1 AS key1, key2 AS key2, .. keyn AS keys,
	///         aggf1(agg1) AS agg1, aggfn(aggn) AS aggn
	///     FROM (
	///             SELECT
	///                 kexp1 AS key1, kexp2 AS key2,... kexpn AS keyn,
	///                 aexpr1 AS agg1, .. aexprn AS aggn
	///             FROM input AS a
	///         ) as a
	///     GROUP BY key1, key2, keyn
	///
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlSelectStatement"/></returns>
	public override ISqlFragment Visit(DbGroupByExpression e)
	{
		var varName = GetShortenedName(e.Input.VariableName);
		var innerQuery = VisitInputExpression(e.Input.Expression,
			varName, e.Input.VariableType, out var fromSymbol);

		// GroupBy is compatible with Filter and OrderBy
		// but not with Project, GroupBy
		if (!IsCompatible(innerQuery, e.ExpressionKind))
		{
			innerQuery = CreateNewSelectStatement(innerQuery, varName, e.Input.VariableType, out fromSymbol);
		}

		_selectStatementStack.Push(innerQuery);
		_symbolTable.EnterScope();

		AddFromSymbol(innerQuery, varName, fromSymbol);
		// This line is not present for other relational nodes.
		_symbolTable.Add(GetShortenedName(e.Input.GroupVariableName), fromSymbol);


		// The enumerator is shared by both the keys and the aggregates,
		// so, we do not close it in between.
		var groupByType = MetadataHelpers.GetEdmType<RowType>(MetadataHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage);

		// Whenever there exists at least one aggregate with an argument that is not simply a PropertyExpression
		// over a VarRefExpression, we need a nested query in which we alias the arguments to the aggregates.
		var needsInnerQuery = NeedsInnerQuery(e.Aggregates);

		SqlSelectStatement result;
		if (needsInnerQuery)
		{
			//Create the inner query
			result = CreateNewSelectStatement(innerQuery, varName, e.Input.VariableType, false, out fromSymbol);
			AddFromSymbol(result, varName, fromSymbol, false);
		}
		else
		{
			result = innerQuery;
		}

		using (IEnumerator<EdmProperty> members = groupByType.Properties.GetEnumerator())
		{
			members.MoveNext();
			Debug.Assert(result.Select.IsEmpty);

			var separator = string.Empty;

			foreach (var key in e.Keys)
			{
				var member = members.Current;
				var alias = QuoteIdentifier(member.Name);

				result.GroupBy.Append(separator);

				var keySql = key.Accept(this);

				if (!needsInnerQuery)
				{
					//Default translation: Key AS Alias
					result.Select.Append(separator);
					result.Select.AppendLine();
					result.Select.Append(keySql);
					result.Select.Append(" AS ");
					result.Select.Append(alias);

					result.GroupBy.Append(keySql);
				}
				else
				{
					// The inner query contains the default translation Key AS Alias
					innerQuery.Select.Append(separator);
					innerQuery.Select.AppendLine();
					innerQuery.Select.Append(keySql);
					innerQuery.Select.Append(" AS ");
					innerQuery.Select.Append(alias);

					//The outer resulting query projects over the key aliased in the inner query:
					//  fromSymbol.Alias AS Alias
					result.Select.Append(separator);
					result.Select.AppendLine();
					result.Select.Append(fromSymbol);
					result.Select.Append(".");
					result.Select.Append(alias);
					result.Select.Append(" AS ");
					result.Select.Append(alias);

					result.GroupBy.Append(alias);
				}

				separator = ", ";
				members.MoveNext();
			}

			foreach (var aggregate in e.Aggregates)
			{
				var member = members.Current;
				var alias = QuoteIdentifier(member.Name);

				Debug.Assert(aggregate.Arguments.Count == 1);
				var translatedAggregateArgument = aggregate.Arguments[0].Accept(this);

				object aggregateArgument;

				if (needsInnerQuery)
				{
					//In this case the argument to the aggratete is reference to the one projected out by the
					// inner query
					var wrappingAggregateArgument = new SqlBuilder();
					wrappingAggregateArgument.Append(fromSymbol);
					wrappingAggregateArgument.Append(".");
					wrappingAggregateArgument.Append(alias);
					aggregateArgument = wrappingAggregateArgument;

					innerQuery.Select.Append(separator);
					innerQuery.Select.AppendLine();
					innerQuery.Select.Append(translatedAggregateArgument);
					innerQuery.Select.Append(" AS ");
					innerQuery.Select.Append(alias);
				}
				else
				{
					aggregateArgument = translatedAggregateArgument;
				}

				ISqlFragment aggregateResult = VisitAggregate(aggregate, aggregateArgument);

				result.Select.Append(separator);
				result.Select.AppendLine();
				result.Select.Append(aggregateResult);
				result.Select.Append(" AS ");
				result.Select.Append(alias);

				separator = ", ";
				members.MoveNext();
			}
		}


		_symbolTable.ExitScope();
		_selectStatementStack.Pop();

		return result;
	}

	/// <summary>
	/// <see cref="Visit(DbUnionAllExpression)"/>
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	public override ISqlFragment Visit(DbIntersectExpression e)
	{
		throw new NotSupportedException("The INTERSECT statement is not supported in Firebird.");
		//return VisitSetOpExpression(e.Left, e.Right, "INTERSECT");
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
		return VisitIsEmptyExpression(e, false);
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
		return VisitIsNullExpression(e, false);
	}

	/// <summary>
	/// <see cref="DbIsOfExpression"/> is illegal at this stage
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
		return VisitJoinExpression(e.Inputs, e.ExpressionKind, "CROSS JOIN", null);
	}

	/// <summary>
	/// <see cref="VisitJoinExpression"/>
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlSelectStatement"/>.</returns>
	public override ISqlFragment Visit(DbJoinExpression e)
	{
		#region Map join type to a string
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
		#endregion

		var inputs = new List<DbExpressionBinding>(2);
		inputs.Add(e.Left);
		inputs.Add(e.Right);

		return VisitJoinExpression(inputs, e.ExpressionKind, joinString, e.JoinCondition);
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/></returns>
	public override ISqlFragment Visit(DbLikeExpression e)
	{
		var result = new SqlBuilder();

		if (!(e.Argument is DbParameterReferenceExpression && e.Pattern is DbParameterReferenceExpression))
		{
			_shouldCastParameter = false;
		}

		result.Append(e.Argument.Accept(this));
		result.Append(" LIKE ");
		result.Append(e.Pattern.Accept(this));

		// if the ESCAPE expression is a DbNullExpression, then that's tantamount to
		// not having an ESCAPE at all
		if (e.Escape.ExpressionKind != DbExpressionKind.Null)
		{
			result.Append(" ESCAPE ");
			result.Append(e.Escape.Accept(this));
		}

		_shouldCastParameter = true;

		return result;
	}

	/// <summary>
	///  Translates to TOP expression.
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/></returns>
	public override ISqlFragment Visit(DbLimitExpression e)
	{
		Debug.Assert(e.Limit is DbConstantExpression || e.Limit is DbParameterReferenceExpression, "DbLimitExpression.Limit is of invalid expression type");

		var result = VisitExpressionEnsureSqlStatement(e.Argument, false);

		if (!IsCompatible(result, e.ExpressionKind))
		{
			var inputType = MetadataHelpers.GetElementTypeUsage(e.Argument.ResultType);

			result = CreateNewSelectStatement(result, "top", inputType, out var fromSymbol);
			AddFromSymbol(result, "top", fromSymbol, false);
		}

		var topCount = HandleCountExpression(e.Limit);

		result.First = new FirstClause(topCount);
		return result;
	}

	/// <summary>
	/// DbNewInstanceExpression is allowed as a child of DbProjectExpression only.
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
	/// <item><see cref="DbNotExpression"/>NOT(Not(x)) becomes x</item>
	/// <item><see cref="DbIsEmptyExpression"/>NOT EXISTS becomes EXISTS</item>
	/// <item><see cref="DbIsNullExpression"/>IS NULL becomes IS NOT NULL</item>
	/// <item><see cref="DbComparisonExpression"/>= becomes&lt;&gt; </item>
	/// </list>
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/></returns>
	public override ISqlFragment Visit(DbNotExpression e)
	{
		// Flatten Not(Not(x)) to x.
		if (e.Argument is DbNotExpression notExpression)
		{
			return notExpression.Argument.Accept(this);
		}

		if (e.Argument is DbIsEmptyExpression isEmptyExpression)
		{
			return VisitIsEmptyExpression(isEmptyExpression, true);
		}

		if (e.Argument is DbIsNullExpression isNullExpression)
		{
			return VisitIsNullExpression(isNullExpression, true);
		}

		if (e.Argument is DbComparisonExpression comparisonExpression)
		{
			if (comparisonExpression.ExpressionKind == DbExpressionKind.Equals)
			{
				return VisitBinaryExpression(" <> ", comparisonExpression.Left, comparisonExpression.Right);
			}
		}

		var result = new SqlBuilder();
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
		var result = new SqlBuilder();
		result.Append("NULL");
		return result;
	}

	/// <summary>
	/// <see cref="DbOfTypeExpression"/> is illegal at this stage
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
	/// <seealso cref="Visit(DbAndExpression)"/>
	public override ISqlFragment Visit(DbOrExpression e)
	{
		return VisitBinaryExpression(" OR ", e.Left, e.Right);
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/></returns>
	public override ISqlFragment Visit(DbParameterReferenceExpression e)
	{
		var result = new SqlBuilder();

		string sqlPrimitiveType = null;
		if (_shouldCastParameter)
		{
			sqlPrimitiveType = GetSqlPrimitiveType(e.ResultType);
			result.Append("CAST(");
		}
		// Do not quote this name.
		// We are not checking that e.Name has no illegal characters. e.g. space
		result.Append("@" + e.ParameterName);
		if (_shouldCastParameter)
		{
			result.Append(" AS ");
			result.Append(sqlPrimitiveType);
			result.Append(")");
		}

		return result;
	}

	/// <summary>
	/// <see cref="Visit(DbFilterExpression)"/> for the general ideas.
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlSelectStatement"/></returns>
	/// <seealso cref="Visit(DbFilterExpression)"/>
	public override ISqlFragment Visit(DbProjectExpression e)
	{
		var varName = GetShortenedName(e.Input.VariableName);
		var result = VisitInputExpression(e.Input.Expression, varName, e.Input.VariableType, out var fromSymbol);

		// Project is compatible with Filter
		// but not with Project, GroupBy
		if (!IsCompatible(result, e.ExpressionKind))
		{
			result = CreateNewSelectStatement(result, varName, e.Input.VariableType, out fromSymbol);
		}

		_selectStatementStack.Push(result);
		_symbolTable.EnterScope();

		AddFromSymbol(result, varName, fromSymbol);

		// Project is the only node that can have DbNewInstanceExpression as a child
		// so we have to check it here.
		// We call VisitNewInstanceExpression instead of Visit(DbNewInstanceExpression), since
		// the latter throws.
		if (e.Projection is DbNewInstanceExpression newInstanceExpression)
		{
			result.Select.Append(VisitNewInstanceExpression(newInstanceExpression));
		}
		else
		{
			result.Select.Append(e.Projection.Accept(this));
		}

		_symbolTable.ExitScope();
		_selectStatementStack.Pop();

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
		SqlBuilder result;
		var varName = e.Property.Name;

		var instanceSql = e.Instance.Accept(this);

		// Since the DbVariableReferenceExpression is a proper child of ours, we can reset
		// isVarSingle.
		if (e.Instance is DbVariableReferenceExpression DbVariableReferenceExpression)
		{
			_isVarRefSingle = false;
		}

		// We need to flatten, and have not yet seen the first nested SELECT statement.
		if (instanceSql is JoinSymbol joinSymbol)
		{
			varName = GetShortenedName(varName);
			Debug.Assert(joinSymbol.NameToExtent.ContainsKey(varName));
			if (joinSymbol.IsNestedJoin)
			{
				return new SymbolPair(joinSymbol, joinSymbol.NameToExtent[varName]);
			}
			else
			{
				return joinSymbol.NameToExtent[varName];
			}
		}
		// ---------------------------------------
		// We have seen the first nested SELECT statement, but not the column.
		if (instanceSql is SymbolPair symbolPair)
		{
			varName = GetShortenedName(varName);
			if (symbolPair.Column is JoinSymbol columnJoinSymbol)
			{
				Debug.Assert(columnJoinSymbol.NameToExtent.ContainsKey(varName));
				symbolPair.Column = columnJoinSymbol.NameToExtent[varName];
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
		result.Append(QuoteIdentifier(varName));

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
		var result = new SqlBuilder();

		var negatePredicate = (e.ExpressionKind == DbExpressionKind.All);
		if (e.ExpressionKind == DbExpressionKind.Any)
		{
			result.Append("EXISTS (");
		}
		else
		{
			Debug.Assert(e.ExpressionKind == DbExpressionKind.All);
			result.Append("NOT EXISTS (");
		}

		var filter = VisitFilterExpression(e.Input, e.Predicate, negatePredicate);
		if (filter.Select.IsEmpty)
		{
			AddDefaultColumns(filter);
		}

		result.Append(filter);
		result.Append(")");

		return result;
	}

	/// <summary>
	/// <see cref="DbRefExpression"/> is illegal at this stage
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	public override ISqlFragment Visit(DbRefExpression e)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// <see cref="DbRelationshipNavigationExpression"/> is illegal at this stage
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	public override ISqlFragment Visit(DbRelationshipNavigationExpression e)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	///  Translates to SKIP expression.
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/></returns>
	public override ISqlFragment Visit(DbSkipExpression e)
	{
		Debug.Assert(e.Count is DbConstantExpression || e.Count is DbParameterReferenceExpression, "DbSkipExpression.Count is of invalid expression type");

		var varName = GetShortenedName(e.Input.VariableName);
		var result = VisitInputExpression(e.Input.Expression, varName, e.Input.VariableType, out var fromSymbol);

		if (!IsCompatible(result, e.ExpressionKind))
		{
			var inputType = MetadataHelpers.GetElementTypeUsage(e.ResultType);

			result = CreateNewSelectStatement(result, varName, inputType, out fromSymbol);
			AddFromSymbol(result, varName, fromSymbol, false);
		}

		var skipCount = HandleCountExpression(e.Count);

		result.Skip = new SkipClause(skipCount);

		_selectStatementStack.Push(result);
		_symbolTable.EnterScope();

		AddFromSymbol(result, varName, fromSymbol);

		AddSortKeys(result.OrderBy, e.SortOrder);

		_symbolTable.ExitScope();
		_selectStatementStack.Pop();

		return result;
	}

	/// <summary>
	/// <see cref="Visit(DbFilterExpression)"/>
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlSelectStatement"/></returns>
	/// <seealso cref="Visit(DbFilterExpression)"/>
	public override ISqlFragment Visit(DbSortExpression e)
	{
		var varName = GetShortenedName(e.Input.VariableName);
		var result = VisitInputExpression(e.Input.Expression, varName, e.Input.VariableType, out var fromSymbol);

		// OrderBy is compatible with Filter
		// and nothing else
		if (!IsCompatible(result, e.ExpressionKind))
		{
			result = CreateNewSelectStatement(result, varName, e.Input.VariableType, out fromSymbol);
		}

		_selectStatementStack.Push(result);
		_symbolTable.EnterScope();

		AddFromSymbol(result, varName, fromSymbol);

		AddSortKeys(result.OrderBy, e.SortOrder);

		_symbolTable.ExitScope();
		_selectStatementStack.Pop();

		return result;
	}

	/// <summary>
	/// <see cref="DbTreatExpression"/> is illegal at this stage
	/// </summary>
	/// <param name="e"></param>
	/// <returns>A <see cref="SqlBuilder"/></returns>
	public override ISqlFragment Visit(DbTreatExpression e)
	{
		throw new NotSupportedException();
	}

	/// <summary>
	/// This code is shared by <see cref="Visit(DbExceptExpression)"/>
	/// and <see cref="Visit(DbIntersectExpression)"/>
	///
	/// <see cref="VisitSetOpExpression"/>
	/// Since the left and right expression may not be Sql select statements,
	/// we must wrap them up to look like SQL select statements.
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	public override ISqlFragment Visit(DbUnionAllExpression e)
	{
		return VisitSetOpExpression(e.Left, e.Right, "UNION ALL");
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
		if (_isVarRefSingle)
		{
			throw new NotSupportedException();
			// A DbVariableReferenceExpression has to be a child of DbPropertyExpression or MethodExpression
			// This is also checked in GenerateSql(...) at the end of the visiting.
		}
		_isVarRefSingle = true; // This will be reset by DbPropertyExpression or MethodExpression

		var varName = GetShortenedName(e.VariableName);
		var result = _symbolTable.Lookup(varName);
		if (!CurrentSelectStatement.FromExtents.Contains(result))
		{
			CurrentSelectStatement.OuterExtents[result] = true;
		}

		return result;
	}

	public override ISqlFragment Visit(DbInExpression e)
	{
		var result = new SqlBuilder();

		result.Append(e.Item.Accept(this));
		result.Append(" IN (");

		var separator = string.Empty;
		foreach (var item in e.List)
		{
			result.Append(separator);
			result.Append(item.Accept(this));

			separator = ",";
		}

		result.Append(")");

		return result;
	}

	#region Visits shared by multiple nodes
	/// <summary>
	/// Aggregates are not visited by the normal visitor walk.
	/// </summary>
	/// <param name="aggregate">The aggreate go be translated</param>
	/// <param name="aggregateArgument">The translated aggregate argument</param>
	/// <returns></returns>
	SqlBuilder VisitAggregate(DbAggregate aggregate, object aggregateArgument)
	{
		var aggregateResult = new SqlBuilder();

		if (!(aggregate is DbFunctionAggregate functionAggregate))
		{
			throw new NotSupportedException();
		}

		if (MetadataHelpers.IsCanonicalFunction(functionAggregate.Function) && (
			string.Equals(functionAggregate.Function.Name, "StDev", StringComparison.Ordinal) ||
			string.Equals(functionAggregate.Function.Name, "StDevP", StringComparison.Ordinal) ||
			string.Equals(functionAggregate.Function.Name, "Var", StringComparison.Ordinal) ||
			string.Equals(functionAggregate.Function.Name, "VarP", StringComparison.Ordinal)))
		{
			throw new NotSupportedException();
		}

		WriteFunctionName(aggregateResult, functionAggregate.Function);

		aggregateResult.Append("(");

		if (functionAggregate.Distinct)
		{
			aggregateResult.Append("DISTINCT ");
		}

		aggregateResult.Append(aggregateArgument);

		aggregateResult.Append(")");
		return aggregateResult;
	}


	SqlBuilder VisitBinaryExpression(string op, DbExpression left, DbExpression right)
	{
		var result = new SqlBuilder();

		if (!(left is DbParameterReferenceExpression && right is DbParameterReferenceExpression))
		{
			_shouldCastParameter = false;
		}

		if (IsComplexExpression(left))
		{
			result.Append("(");
		}

		result.Append(left.Accept(this));

		if (IsComplexExpression(left))
		{
			result.Append(")");
		}

		if (_shouldHandleBoolComparison)
		{
			result.Append(op);

			if (IsComplexExpression(right))
			{
				result.Append("(");
			}

			result.Append(right.Accept(this));

			if (IsComplexExpression(right))
			{
				result.Append(")");
			}
		}
		else
		{
			_shouldHandleBoolComparison = true;
		}

		_shouldCastParameter = true;

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
	SqlSelectStatement VisitInputExpression(DbExpression inputExpression,
		string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
	{
		SqlSelectStatement result;
		var sqlFragment = inputExpression.Accept(this);
		result = sqlFragment as SqlSelectStatement;

		if (result == null)
		{
			result = new SqlSelectStatement();
			WrapNonQueryExtent(result, sqlFragment, inputExpression.ExpressionKind);
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
			var joinSymbol = new JoinSymbol(inputVarName, inputVarType, result.FromExtents);
			joinSymbol.FlattenedExtentList = result.AllJoinExtents;

			fromSymbol = joinSymbol;
			result.FromExtents.Clear();
			result.FromExtents.Add(fromSymbol);
		}

		return result;
	}

	/// <summary>
	/// <see cref="Visit(DbIsEmptyExpression)"/>
	/// </summary>
	/// <param name="e"></param>
	/// <param name="negate">Was the parent a DbNotExpression?</param>
	/// <returns></returns>
	SqlBuilder VisitIsEmptyExpression(DbIsEmptyExpression e, bool negate)
	{
		var result = new SqlBuilder();
		if (!negate)
		{
			result.Append(" NOT");
		}
		result.Append(" EXISTS (");
		result.Append(VisitExpressionEnsureSqlStatement(e.Argument));
		result.AppendLine();
		result.Append(")");

		return result;
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
			var elementExpr = e.Arguments[0] as DbElementExpression;
			var result = VisitExpressionEnsureSqlStatement(elementExpr.Argument);

			if (!IsCompatible(result, DbExpressionKind.Element))
			{
				var inputType = MetadataHelpers.GetElementTypeUsage(elementExpr.Argument.ResultType);

				result = CreateNewSelectStatement(result, "element", inputType, out var fromSymbol);
				AddFromSymbol(result, "element", fromSymbol, false);
			}

			result.First = new FirstClause(1);
			return result;
		}


		// Otherwise simply build this out as a union-all ladder
		var collectionType = MetadataHelpers.GetEdmType<CollectionType>(e.ResultType);
		Debug.Assert(collectionType != null);
		var isScalarElement = MetadataHelpers.IsPrimitiveType(collectionType.TypeUsage);

		var resultSql = new SqlBuilder();
		var separator = string.Empty;

		// handle empty table
		if (e.Arguments.Count == 0)
		{
			Debug.Assert(isScalarElement);
			resultSql.Append(" SELECT CAST(NULL AS ");
			resultSql.Append(GetSqlPrimitiveType(collectionType.TypeUsage));
			resultSql.Append(") AS X FROM (SELECT 1 FROM RDB$DATABASE) WHERE 1=0");
		}

		foreach (var arg in e.Arguments)
		{
			resultSql.Append(separator);
			resultSql.Append(" SELECT ");
			resultSql.Append(arg.Accept(this));
			// For scalar elements, no alias is appended yet. Add this.
			if (isScalarElement)
			{
				resultSql.Append(" AS X FROM RDB$DATABASE");
			}
			separator = " UNION ALL ";
		}

		return resultSql;
	}


	/// <summary>
	/// <see cref="Visit(DbIsNullExpression)"/>
	/// </summary>
	/// <param name="e"></param>
	/// <param name="negate">Was the parent a DbNotExpression?</param>
	/// <returns></returns>
	SqlBuilder VisitIsNullExpression(DbIsNullExpression e, bool negate)
	{
		var result = new SqlBuilder();
		_shouldCastParameter = false;
		result.Append(e.Argument.Accept(this));
		if (!negate)
		{
			result.Append(" IS NULL");
		}
		else
		{
			result.Append(" IS NOT NULL");
		}
		_shouldCastParameter = true;
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
	/// This method is called from <see cref="Visit(DbApplyExpression)"/> and
	/// <see cref="Visit(DbJoinExpression)"/>.
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
			_selectStatementStack.Push(result);
		}
		else
		{
			result = CurrentSelectStatement;
		}

		// Process each of the inputs, and then the joinCondition if it exists.
		// It would be nice if we could call VisitInputExpression - that would
		// avoid some code duplication
		// but the Join postprocessing is messy and prevents this reuse.
		_symbolTable.EnterScope();

		var separator = string.Empty;
		var isLeftMostInput = true;
		var inputCount = inputs.Count;
		for (var idx = 0; idx < inputCount; idx++)
		{
			var input = inputs[idx];

			if (separator != string.Empty)
			{
				result.From.AppendLine();
			}
			result.From.Append(separator + " ");
			// Change this if other conditions are required
			// to force the child to produce a nested SqlStatement.
			var needsJoinContext = (input.Expression.ExpressionKind == DbExpressionKind.Scan)
									|| (isLeftMostInput &&
										(IsJoinExpression(input.Expression)
										 || IsApplyExpression(input.Expression)))
									;

			_isParentAJoinStack.Push(needsJoinContext ? true : false);
			// if the child reuses our select statement, it will append the from
			// symbols to our FromExtents list.  So, we need to remember the
			// start of the child's entries.
			var fromSymbolStart = result.FromExtents.Count;

			var fromExtentFragment = input.Expression.Accept(this);

			_isParentAJoinStack.Pop();

			ProcessJoinInputResult(fromExtentFragment, result, input, fromSymbolStart);
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
				_isParentAJoinStack.Push(false);
				result.From.Append(joinCondition.Accept(this));
				_isParentAJoinStack.Pop();
				break;
		}

		_symbolTable.ExitScope();

		if (!IsParentAJoin)
		{
			_selectStatementStack.Pop();
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
	void ProcessJoinInputResult(ISqlFragment fromExtentFragment, SqlSelectStatement result,
		DbExpressionBinding input, int fromSymbolStart)
	{
		Symbol fromSymbol = null;
		var varName = GetShortenedName(input.VariableName);

		if (result != fromExtentFragment)
		{
			// The child has its own select statement, and is not reusing
			// our select statement.
			// This should look a lot like VisitInputExpression().
			if (fromExtentFragment is SqlSelectStatement sqlSelectStatement)
			{
				if (sqlSelectStatement.Select.IsEmpty)
				{
					var columns = AddDefaultColumns(sqlSelectStatement);

					if (IsJoinExpression(input.Expression)
						|| IsApplyExpression(input.Expression))
					{
						var extents = sqlSelectStatement.FromExtents;
						var newJoinSymbol = new JoinSymbol(varName, input.VariableType, extents);
						newJoinSymbol.IsNestedJoin = true;
						newJoinSymbol.ColumnList = columns;

						fromSymbol = newJoinSymbol;
					}
					else
					{
						// this is a copy of the code in CreateNewSelectStatement.

						// if the oldStatement has a join as its input, ...
						// clone the join symbol, so that we "reuse" the
						// join symbol.  Normally, we create a new symbol - see the next block
						// of code.
						if (sqlSelectStatement.FromExtents[0] is JoinSymbol oldJoinSymbol)
						{
							// Note: sqlSelectStatement.FromExtents will not do, since it might
							// just be an alias of joinSymbol, and we want an actual JoinSymbol.
							var newJoinSymbol = new JoinSymbol(varName, input.VariableType, oldJoinSymbol.ExtentList);
							// This indicates that the sqlSelectStatement is a blocking scope
							// i.e. it hides/renames extent columns
							newJoinSymbol.IsNestedJoin = true;
							newJoinSymbol.ColumnList = columns;
							newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;

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
				WrapNonQueryExtent(result, fromExtentFragment, input.Expression.ExpressionKind);
			}

			if (fromSymbol == null) // i.e. not a join symbol
			{
				fromSymbol = new Symbol(varName, input.VariableType);
			}


			AddFromSymbol(result, varName, fromSymbol);
			result.AllJoinExtents.Add(fromSymbol);
		}
		else // result == fromExtentFragment.  The child extents have been merged into the parent's.
		{
			// we are adding extents to the current sql statement via flattening.
			// We are replacing the child's extents with a single Join symbol.
			// The child's extents are all those following the index fromSymbolStart.
			//
			var extents = new List<Symbol>();

			// We cannot call extents.AddRange, since the is no simple way to
			// get the range of symbols fromSymbolStart..result.FromExtents.Count
			// from result.FromExtents.
			// We copy these symbols to create the JoinSymbol later.
			for (var i = fromSymbolStart; i < result.FromExtents.Count; ++i)
			{
				extents.Add(result.FromExtents[i]);
			}
			result.FromExtents.RemoveRange(fromSymbolStart, result.FromExtents.Count - fromSymbolStart);
			fromSymbol = new JoinSymbol(varName, input.VariableType, extents);
			result.FromExtents.Add(fromSymbol);
			// this Join Symbol does not have its own select statement, so we
			// do not set IsNestedJoin


			// We do not call AddFromSymbol(), since we do not want to add
			// "AS alias" to the FROM clause- it has been done when the extent was added earlier.
			_symbolTable.Add(varName, fromSymbol);
		}
	}

	/// <summary>
	/// We assume that this is only called as a child of a Project.
	///
	/// This replaces <see cref="Visit(DbNewInstanceExpression)"/>, since
	/// we do not allow DbNewInstanceExpression as a child of any node other than
	/// DbProjectExpression.
	///
	/// We write out the translation of each of the columns in the record.
	/// </summary>
	/// <param name="e"></param>

	/// <returns>A <see cref="SqlBuilder"/></returns>
	ISqlFragment VisitNewInstanceExpression(DbNewInstanceExpression e)
	{
		var result = new SqlBuilder();

		if (e.ResultType.EdmType is RowType rowType)
		{
			var members = rowType.Properties;
			var separator = string.Empty;
			for (var i = 0; i < e.Arguments.Count; ++i)
			{
				var argument = e.Arguments[i];
				if (MetadataHelpers.IsRowType(argument.ResultType))
				{
					// We do not support nested records or other complex objects.
					throw new NotSupportedException();
				}

				var member = members[i];
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

	ISqlFragment VisitSetOpExpression(DbExpression left, DbExpression right, string separator)
	{

		var leftSelectStatement = VisitExpressionEnsureSqlStatement(left);
		var rightSelectStatement = VisitExpressionEnsureSqlStatement(right);

		var setStatement = new SqlBuilder();
		setStatement.Append(leftSelectStatement);
		setStatement.AppendLine();
		setStatement.Append(separator); // e.g. UNION ALL
		setStatement.AppendLine();
		setStatement.Append(rightSelectStatement);

		return setStatement;
	}


	#endregion

	#region Function Handling Helpers
	/// <summary>
	/// Determines whether the given function is a built-in function that requires special handling
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	private bool IsSpecialBuiltInFunction(DbFunctionExpression e)
	{
		return IsBuiltInFunction(e.Function) && _builtInFunctionHandlers.ContainsKey(e.Function.Name);
	}

	/// <summary>
	/// Determines whether the given function is a canonical function that requires special handling
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	private bool IsSpecialCanonicalFunction(DbFunctionExpression e)
	{
		return MetadataHelpers.IsCanonicalFunction(e.Function) && _canonicalFunctionHandlers.ContainsKey(e.Function.Name);
	}

	/// <summary>
	/// Default handling for functions
	/// Translates them to FunctionName(arg1, arg2, ..., argn)
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	private ISqlFragment HandleFunctionDefault(DbFunctionExpression e)
	{
		var result = new SqlBuilder();
		WriteFunctionName(result, e.Function);
		HandleFunctionArgumentsDefault(e, result);
		return result;
	}

	/// <summary>
	/// Default handling for functions with a given name.
	/// Translates them to functionName(arg1, arg2, ..., argn)
	/// </summary>
	/// <param name="e"></param>
	/// <param name="functionName"></param>
	/// <returns></returns>
	private ISqlFragment HandleFunctionDefaultGivenName(DbFunctionExpression e, string functionName)
	{
		var result = new SqlBuilder();
		result.Append(functionName);
		HandleFunctionArgumentsDefault(e, result);
		return result;
	}

	/// <summary>
	/// Default handling on function arguments
	/// Appends the list of arguments to the given result
	/// If the function is niladic it does not append anything,
	/// otherwise it appends (arg1, arg2, ..., argn)
	/// </summary>
	/// <param name="e"></param>
	/// <param name="result"></param>
	private void HandleFunctionArgumentsDefault(DbFunctionExpression e, SqlBuilder result)
	{
		var isNiladicFunction = MetadataHelpers.TryGetValueForMetadataProperty<bool>(e.Function, "NiladicFunctionAttribute");
		if (isNiladicFunction && e.Arguments.Count > 0)
		{
			throw new InvalidOperationException("Niladic functions cannot have parameters");
		}

		if (!isNiladicFunction)
		{
			result.Append("(");
			var separator = string.Empty;
			foreach (var arg in e.Arguments)
			{
				result.Append(separator);
				result.Append(arg.Accept(this));
				separator = ", ";
			}
			result.Append(")");
		}
	}

	/// <summary>
	/// Handler for special built in functions
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	private ISqlFragment HandleSpecialBuiltInFunction(DbFunctionExpression e)
	{
		return HandleSpecialFunction(_builtInFunctionHandlers, e);
	}

	/// <summary>
	/// Handler for special canonical functions
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	private ISqlFragment HandleSpecialCanonicalFunction(DbFunctionExpression e)
	{
		return HandleSpecialFunction(_canonicalFunctionHandlers, e);
	}

	/// <summary>
	/// Dispatches the special function processing to the appropriate handler
	/// </summary>
	/// <param name="handlers"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	private ISqlFragment HandleSpecialFunction(Dictionary<string, FunctionHandler> handlers, DbFunctionExpression e)
	{
		if (!handlers.ContainsKey(e.Function.Name))
			throw new InvalidOperationException("Special handling should be called only for functions in the list of special functions");

		return handlers[e.Function.Name](this, e);
	}

	/// <summary>
	/// Handles functions that are translated into SQL operators.
	/// The given function should have one or two arguments.
	/// Functions with one arguemnt are translated into
	///     op arg
	/// Functions with two arguments are translated into
	///     arg0 op arg1
	/// Also, the arguments can be optionaly enclosed in parethesis
	/// </summary>
	/// <param name="e"></param>
	/// <param name="parenthesiseArguments">Whether the arguments should be enclosed in parethesis</param>
	/// <returns></returns>
	private ISqlFragment HandleSpecialFunctionToOperator(DbFunctionExpression e, bool parenthesiseArguments)
	{
		var result = new SqlBuilder();
		Debug.Assert(e.Arguments.Count > 0 && e.Arguments.Count <= 2, "There should be 1 or 2 arguments for operator");

		if (e.Arguments.Count > 1)
		{
			if (parenthesiseArguments)
			{
				result.Append("(");
			}
			result.Append(e.Arguments[0].Accept(this));
			if (parenthesiseArguments)
			{
				result.Append(")");
			}
		}
		result.Append(" ");
		Debug.Assert(_functionNameToOperatorDictionary.ContainsKey(e.Function.Name), "The function can not be mapped to an operator");
		result.Append(_functionNameToOperatorDictionary[e.Function.Name]);
		result.Append(" ");

		if (parenthesiseArguments)
		{
			result.Append("(");
		}
		result.Append(e.Arguments[e.Arguments.Count - 1].Accept(this));
		if (parenthesiseArguments)
		{
			result.Append(")");
		}
		return result;
	}

	#region String Canonical Functions
	private static ISqlFragment HandleCanonicalConcatFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleSpecialFunctionToOperator(e, false);
	}

	private static ISqlFragment HandleCanonicalContainsFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen._shouldHandleBoolComparison = false;
		return sqlgen.HandleSpecialFunctionToOperator(e, false);
	}

	private static ISqlFragment HandleCanonicalEndsWithFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen._shouldHandleBoolComparison = false;
		var result = new SqlBuilder();
		result.Append("REVERSE(");
		result.Append(e.Arguments[0].Accept(sqlgen));
		result.Append(") STARTING WITH REVERSE(");
		result.Append(e.Arguments[1].Accept(sqlgen));
		result.Append(")");
		return result;
	}

	private static ISqlFragment HandleCanonicalFunctionIndexOf(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "POSITION");
	}

	private static ISqlFragment HandleCanonicalFunctionLength(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "CHAR_LENGTH");
	}

	private static ISqlFragment HandleCanonicalFunctionTrim(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return TrimHelper(sqlgen, e, "BOTH");
	}

	private static ISqlFragment HandleCanonicalFunctionLTrim(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return TrimHelper(sqlgen, e, "LEADING");
	}

	private static ISqlFragment HandleCanonicalFunctionRTrim(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return TrimHelper(sqlgen, e, "TRAILING");
	}

	/// <summary>
	/// TRIM ( [ [ <trim specification> ] [ <trim character> ] FROM ] <value expression> )
	/// <trim specification> ::=  LEADING  | TRAILING  | BOTH
	/// </summary>
	private static ISqlFragment TrimHelper(SqlGenerator sqlgen, DbFunctionExpression e, string what)
	{
		var result = new SqlBuilder();

		result.Append("TRIM(");
		result.Append(what);
		result.Append(" FROM ");

		Debug.Assert(e.Arguments.Count == 1, "Trim should have one argument");
		result.Append(e.Arguments[0].Accept(sqlgen));

		result.Append(")");

		return result;
	}

	private static ISqlFragment HandleCanonicalFunctionLeft(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "LEFT");
	}

	private static ISqlFragment HandleCanonicalFunctionRight(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "RIGHT");
	}

	private static ISqlFragment HandleCanonicalFunctionReverse(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "REVERSE");
	}

	private static ISqlFragment HandleCanonicalFunctionReplace(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "REPLACE");
	}

	private static ISqlFragment HandleCanonicalStartsWithFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		sqlgen._shouldHandleBoolComparison = false;
		return sqlgen.HandleSpecialFunctionToOperator(e, false);
	}

	private static ISqlFragment HandleCanonicalFunctionSubstring(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		var result = new SqlBuilder();

		result.Append("SUBSTRING(");

		Debug.Assert(e.Arguments.Count == 3, "Substring should have three arguments");
		result.Append(e.Arguments[0].Accept(sqlgen));
		result.Append(" FROM ");
		result.Append(e.Arguments[1].Accept(sqlgen));
		result.Append(" FOR ");
		result.Append(e.Arguments[2].Accept(sqlgen));

		result.Append(")");

		return result;
	}

	private static ISqlFragment HandleCanonicalFunctionToLower(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "LOWER");
	}

	private static ISqlFragment HandleCanonicalFunctionToUpper(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "UPPER");
	}
	#endregion

	#region Bitwise Canonical Functions
	private static ISqlFragment HandleCanonicalFunctionBitwiseAnd(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "BIN_AND");
	}

	private static ISqlFragment HandleCanonicalFunctionBitwiseNot(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "BIN_NOT");
	}

	private static ISqlFragment HandleCanonicalFunctionBitwiseOr(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "BIN_OR");
	}

	private static ISqlFragment HandleCanonicalFunctionBitwiseXor(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "BIN_XOR");
	}
	#endregion

	#region Date and Time Canonical Functions
	private static ISqlFragment HandleCanonicalFunctionCurrentUtcDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		throw new NotSupportedException("CurrentUtcDateTime is not supported by Firebird.");
	}

	private static ISqlFragment HandleCanonicalFunctionCurrentDateTimeOffset(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		throw new NotSupportedException("CurrentDateTimeOffset is not supported by Firebird.");
	}

	private static ISqlFragment HandleCanonicalFunctionGetTotalOffsetMinutes(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		throw new NotSupportedException("GetTotalOffsetMinutes is not supported by Firebird.");
	}

	private static ISqlFragment HandleCanonicalFunctionCurrentDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		var result = new SqlBuilder();
		result.Append("CURRENT_TIMESTAMP");
		return result;
	}

	/// <summary>
	/// Handler for canonical funcitons for extracting date parts.
	/// For example:
	///     Year(date) -> EXTRACT(YEAR from date)
	/// </summary>
	private static ISqlFragment HandleCanonicalFunctionExtract(SqlGenerator sqlgen, DbFunctionExpression e, string extractPart)
	{
		if (extractPart == null)
			throw new NotSupportedException();

		var result = new SqlBuilder();
		result.Append("EXTRACT(");
		result.Append(extractPart);
		result.Append(" FROM ");
		Debug.Assert(e.Arguments.Count == 1, "Canonical datepart functions should have exactly one argument");
		result.Append(e.Arguments[0].Accept(sqlgen));
		result.Append(")");
		return result;
	}

	private static ISqlFragment HandleCanonicalFunctionDateTimeAdd(SqlGenerator sqlgen, DbFunctionExpression e, string addPart)
	{
		if (addPart == null)
			throw new NotSupportedException();

		var result = new SqlBuilder();
		result.Append("DATEADD(");
		result.Append(addPart);
		result.Append(", ");
		Debug.Assert(e.Arguments.Count == 2, "Canonical dateadd functions should have exactly two arguments");
		result.Append(e.Arguments[1].Accept(sqlgen));
		result.Append(", ");
		result.Append(e.Arguments[0].Accept(sqlgen));
		result.Append(")");
		return result;
	}

	private static ISqlFragment HandleCanonicalFunctionDateTimeDiff(SqlGenerator sqlgen, DbFunctionExpression e, string diffPart)
	{
		if (diffPart == null)
			throw new NotSupportedException();

		var result = new SqlBuilder();
		result.Append("DATEDIFF(");
		result.Append(diffPart);
		result.Append(", ");
		Debug.Assert(e.Arguments.Count == 2, "Canonical datediff functions should have exactly two arguments");
		result.Append(e.Arguments[1].Accept(sqlgen));
		result.Append(", ");
		result.Append(e.Arguments[0].Accept(sqlgen));
		result.Append(")");
		return result;
	}

	/// <summary>
	/// CCYY-MM-DD HH:NN:SS.nnnn
	/// CreateDateTime(year, month, day, hour, minute, second)
	/// </summary>
	private static ISqlFragment HandleCanonicalFunctionCreateDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		var result = new SqlBuilder();

		result.Append("CAST('");
		result.Append((e.Arguments[0].ExpressionKind == DbExpressionKind.Constant) ? e.Arguments[0].Accept(sqlgen) : (object)"0001"); // year
		result.Append("-");
		result.Append((e.Arguments[1].ExpressionKind == DbExpressionKind.Constant) ? e.Arguments[1].Accept(sqlgen) : (object)"01"); // month
		result.Append("-");
		result.Append((e.Arguments[2].ExpressionKind == DbExpressionKind.Constant) ? e.Arguments[2].Accept(sqlgen) : (object)"01"); // day
		result.Append(" ");
		result.Append((e.Arguments[3].ExpressionKind == DbExpressionKind.Constant) ? e.Arguments[3].Accept(sqlgen) : (object)"00"); // hour
		result.Append(":");
		result.Append((e.Arguments[4].ExpressionKind == DbExpressionKind.Constant) ? e.Arguments[4].Accept(sqlgen) : (object)"00"); // minute
		result.Append(":");
		result.Append("00"); // second is typeof(double?), would result in CAST SqlFragment
		result.Append("' AS TIMESTAMP)");

		// in case a date part is not constant, generate additional DATEADD fragments
		if (e.Arguments[0].ExpressionKind != DbExpressionKind.Constant && e.Arguments[0].ExpressionKind != DbExpressionKind.Null)
			result = HandleDateAdd("YEAR", e.Arguments[0].Accept(sqlgen), result);
		if (e.Arguments[1].ExpressionKind != DbExpressionKind.Constant && e.Arguments[1].ExpressionKind != DbExpressionKind.Null)
			result = HandleDateAdd("MONTH", e.Arguments[1].Accept(sqlgen), result);
		if (e.Arguments[2].ExpressionKind != DbExpressionKind.Constant && e.Arguments[2].ExpressionKind != DbExpressionKind.Null)
			result = HandleDateAdd("DAY", e.Arguments[2].Accept(sqlgen), result);
		if (e.Arguments[3].ExpressionKind != DbExpressionKind.Constant && e.Arguments[3].ExpressionKind != DbExpressionKind.Null)
			result = HandleDateAdd("HOUR", e.Arguments[3].Accept(sqlgen), result);
		if (e.Arguments[4].ExpressionKind != DbExpressionKind.Constant && e.Arguments[4].ExpressionKind != DbExpressionKind.Null)
			result = HandleDateAdd("MINUTE", e.Arguments[4].Accept(sqlgen), result);
		if ((e.Arguments[5].ExpressionKind != DbExpressionKind.Constant || (((DbConstantExpression)e.Arguments[5]).Value as double?) != 0) && e.Arguments[5].ExpressionKind != DbExpressionKind.Null)
			result = HandleDateAdd("SECOND", e.Arguments[5].Accept(sqlgen), result);

		// in case a default value was used for the year/month/day part, remove it afterwards
		if (e.Arguments[0].ExpressionKind != DbExpressionKind.Constant)
			result = HandleDateAdd("YEAR", DbExpression.FromInt32(-1).Accept(sqlgen), result);
		if (e.Arguments[1].ExpressionKind != DbExpressionKind.Constant)
			result = HandleDateAdd("MONTH", DbExpression.FromInt32(-1).Accept(sqlgen), result);
		if (e.Arguments[2].ExpressionKind != DbExpressionKind.Constant)
			result = HandleDateAdd("DAY", DbExpression.FromInt32(-1).Accept(sqlgen), result);

		return result;
	}

	private static SqlBuilder HandleDateAdd(string datePart, ISqlFragment value, ISqlFragment dateTime)
	{
		SqlBuilder result = new SqlBuilder();
		result.Append("DATEADD(");
		result.Append(datePart);
		result.Append(", ");
		result.Append(value);
		result.Append(", ");
		result.Append(dateTime);
		result.Append(")");
		return result;
	}

	private static ISqlFragment HandleCanonicalFunctionCreateDateTimeOffset(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		throw new NotSupportedException("CreateDateTimeOffset is not supported by Firebird.");
	}

	/// <summary>
	/// HH:NN:SS.nnnn
	/// CreateTime(hour, minute, second)
	/// </summary>
	private static ISqlFragment HandleCanonicalFunctionCreateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		var result = new SqlBuilder();
		result.Append("CAST('");
		result.Append(e.Arguments[0].Accept(sqlgen));
		result.Append(":");
		result.Append(e.Arguments[1].Accept(sqlgen));
		result.Append(":");
		result.Append(e.Arguments[2].Accept(sqlgen));
		result.Append("' AS TIME)");
		return result;
	}

	private static ISqlFragment HandleCanonicalFunctionTruncateTime(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		var result = new SqlBuilder();
		result.Append("CAST(CAST(");
		result.Append(e.Arguments[0].Accept(sqlgen));
		result.Append(" as DATE) as TIMESTAMP)");
		return result;
	}
	#endregion

	#region Other Canonical Functions
	private static ISqlFragment HandleCanonicalFunctionNewGuid(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "GEN_UUID");
	}
	#endregion

	#region Math Canonical Functions
	private static ISqlFragment HandleCanonicalFunctionAbs(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "ABS");
	}

	private static ISqlFragment HandleCanonicalFunctionCeiling(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "CEILING");
	}

	private static ISqlFragment HandleCanonicalFunctionFloor(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "FLOOR");
	}

	private static ISqlFragment HandleCanonicalFunctionPower(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "POWER");
	}

	private static ISqlFragment HandleCanonicalFunctionRound(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "ROUND");
	}

	private static ISqlFragment HandleCanonicalFunctionTruncate(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "TRUNC");
	}
	#endregion

	#endregion


	#endregion

	#region Helper methods for the DbExpressionVisitor
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
	void AddColumns(SqlSelectStatement selectStatement, Symbol symbol,
		List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary, ref string separator)
	{
		if (symbol is JoinSymbol joinSymbol)
		{
			if (!joinSymbol.IsNestedJoin)
			{
				// Recurse if the join symbol is a collection of flattened extents
				foreach (var sym in joinSymbol.ExtentList)
				{
					// if sym is ScalarType means we are at base case in the
					// recursion and there are not columns to add, just skip
					if (MetadataHelpers.IsPrimitiveType(sym.Type))
					{
						continue;
					}

					AddColumns(selectStatement, sym, columnList, columnDictionary, ref separator);
				}
			}
			else
			{
				foreach (var joinColumn in joinSymbol.ColumnList)
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

			foreach (var property in MetadataHelpers.GetProperties(symbol.Type))
			{
				var recordMemberName = property.Name;
				// Since all renaming happens in the second phase
				// we lose nothing by setting the next column name index to 0
				// many times.
				_allColumnNames[recordMemberName] = 0;

				// Create a new symbol/reuse existing symbol for the column
				if (!symbol.Columns.TryGetValue(recordMemberName, out var columnSymbol))
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
	List<Symbol> AddDefaultColumns(SqlSelectStatement selectStatement)
	{
		// This is the list of columns added in this select statement
		// This forms the "type" of the Select statement, if it has to
		// be expanded in another SELECT *
		var columnList = new List<Symbol>();

		// A lookup for the previous set of columns to aid column name
		// collision detection.
		var columnDictionary = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);

		var separator = string.Empty;
		// The Select should usually be empty before we are called,
		// but we do not mind if it is not.
		if (!selectStatement.Select.IsEmpty)
		{
			separator = ", ";
		}

		foreach (var symbol in selectStatement.FromExtents)
		{
			AddColumns(selectStatement, symbol, columnList, columnDictionary, ref separator);
		}

		return columnList;
	}

	/// <summary>
	/// <see cref="AddFromSymbol(SqlSelectStatement, string, Symbol, bool)"/>
	/// </summary>
	/// <param name="selectStatement"></param>
	/// <param name="inputVarName"></param>
	/// <param name="fromSymbol"></param>
	void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol)
	{
		AddFromSymbol(selectStatement, inputVarName, fromSymbol, true);
	}

	/// <summary>
	/// This method is called after the input to a relational node is visited.
	/// <see cref="Visit(DbProjectExpression)"/> and <see cref="ProcessJoinInputResult"/>
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
	void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol, bool addToSymbolTable)
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
			_allExtentNames[fromSymbol.Name] = 0;
		}

		if (addToSymbolTable)
		{
			_symbolTable.Add(inputVarName, fromSymbol);
		}
	}

	/// <summary>
	/// Translates a list of SortClauses.
	/// Used in the translation of OrderBy
	/// </summary>
	/// <param name="orderByClause">The SqlBuilder to which the sort keys should be appended</param>
	/// <param name="sortKeys"></param>
	void AddSortKeys(SqlBuilder orderByClause, IList<DbSortClause> sortKeys)
	{
		var separator = string.Empty;
		foreach (var sortClause in sortKeys)
		{
			orderByClause.Append(separator);
			orderByClause.Append(sortClause.Expression.Accept(this));
			Debug.Assert(sortClause.Collation != null);
			if (!string.IsNullOrEmpty(sortClause.Collation))
			{
				orderByClause.Append(" COLLATE ");
				orderByClause.Append(sortClause.Collation);
			}

			orderByClause.Append(sortClause.Ascending ? " ASC" : " DESC");

			separator = ", ";
		}
	}

	/// <summary>
	/// <see cref="CreateNewSelectStatement(SqlSelectStatement oldStatement, string inputVarName, TypeUsage inputVarType, bool finalizeOldStatement, out Symbol fromSymbol) "/>
	/// </summary>
	/// <param name="oldStatement"></param>
	/// <param name="inputVarName"></param>
	/// <param name="inputVarType"></param>
	/// <param name="fromSymbol"></param>
	/// <returns>A new select statement, with the old one as the from clause.</returns>
	SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement,
		string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
	{
		return CreateNewSelectStatement(oldStatement, inputVarName, inputVarType, true, out fromSymbol);
	}


	/// <summary>
	/// This is called after a relational node's input has been visited, and the
	/// input's sql statement cannot be reused.  <see cref="Visit(DbProjectExpression)"/>
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
	/// <param name="finalizeOldStatement"></param>
	/// <param name="fromSymbol"></param>
	/// <returns>A new select statement, with the old one as the from clause.</returns>
	SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement,
		string inputVarName, TypeUsage inputVarType, bool finalizeOldStatement, out Symbol fromSymbol)
	{
		fromSymbol = null;

		// Finalize the old statement
		if (finalizeOldStatement && oldStatement.Select.IsEmpty)
		{
			var columns = AddDefaultColumns(oldStatement);

			// Thid could not have been called from a join node.
			Debug.Assert(oldStatement.FromExtents.Count == 1);

			// if the oldStatement has a join as its input, ...
			// clone the join symbol, so that we "reuse" the
			// join symbol.  Normally, we create a new symbol - see the next block
			// of code.
			if (oldStatement.FromExtents[0] is JoinSymbol oldJoinSymbol)
			{
				// Note: oldStatement.FromExtents will not do, since it might
				// just be an alias of joinSymbol, and we want an actual JoinSymbol.
				var newJoinSymbol = new JoinSymbol(inputVarName, inputVarType, oldJoinSymbol.ExtentList);
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
		var selectStatement = new SqlSelectStatement();
		selectStatement.From.Append("( ");
		selectStatement.From.Append(oldStatement);
		selectStatement.From.AppendLine();
		selectStatement.From.Append(") ");


		return selectStatement;
	}

	internal static string FormatBoolean(bool value)
	{
		return value ? "CAST(1 AS SMALLINT)" : "CAST(0 AS SMALLINT)";
	}

	internal static string FormatBinary(byte[] value)
	{
		return string.Format("x'{0}'", value.ToHexString());
	}

	internal static string FormatString(string value, bool isUnicode, int? explicitLength = null)
	{
		var result = new StringBuilder();
		result.Append("CAST(");
		if (isUnicode)
		{
			result.Append("_UTF8");
		}
		result.Append("'");
		result.Append(value.Replace("'", "''"));
		result.Append("' AS VARCHAR(");
		result.Append(explicitLength ?? value.Length);
		result.Append("))");
		return result.ToString();
	}

	internal static string FormatDateTime(DateTime value)
	{
		var result = new StringBuilder();
		result.Append("CAST('");
		result.Append(value.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture));
		result.Append("' AS TIMESTAMP)");
		return result.ToString();
	}

	internal static string FormatTime(DateTime value)
	{
		var result = new StringBuilder();
		result.Append("CAST('");
		result.Append(value.ToString("HH:mm:ss.ffff", CultureInfo.InvariantCulture));
		result.Append("' AS TIME)");
		return result.ToString();
	}
	internal static string FormatTime(TimeSpan value)
	{
		return FormatTime(DateTime.Today.Add(value));
	}

	internal static string FormatGuid(Guid value)
	{
		var result = new StringBuilder();
		result.Append("CHAR_TO_UUID('");
		result.Append(value.ToString());
		result.Append("')");
		return result.ToString();
	}

	/// <summary>
	/// Returns the sql primitive/native type name.
	/// It will include size, precision or scale depending on type information present in the
	/// type facets
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	internal static string GetSqlPrimitiveType(TypeUsage type)
	{
		var primitiveType = MetadataHelpers.GetEdmType<PrimitiveType>(type);

		var typeName = primitiveType.Name;
		var isUnicode = true;
		var isFixedLength = false;
		var length = 0;
		byte precision = 0;
		byte scale = 0;

		switch (primitiveType.PrimitiveTypeKind)
		{
			case PrimitiveTypeKind.Boolean:
				typeName = "SMALLINT";
				break;

			case PrimitiveTypeKind.Int16:
				typeName = "SMALLINT";
				break;

			case PrimitiveTypeKind.Int32:
				typeName = "INT";
				break;

			case PrimitiveTypeKind.Int64:
				typeName = "BIGINT";
				break;

			case PrimitiveTypeKind.Double:
				typeName = "DOUBLE PRECISION";
				break;

			case PrimitiveTypeKind.Single:
				typeName = "FLOAT";
				break;

			case PrimitiveTypeKind.Decimal:
				precision = MetadataHelpers.GetFacetValueOrDefault<byte>(type, MetadataHelpers.PrecisionFacetName, 9);
				Debug.Assert(precision > 0, "decimal precision must be greater than zero");
				scale = MetadataHelpers.GetFacetValueOrDefault<byte>(type, MetadataHelpers.ScaleFacetName, 0);
				Debug.Assert(precision >= scale, "decimalPrecision must be greater or equal to decimalScale");
				Debug.Assert(precision <= 18, "decimalPrecision must be less than or equal to 18");
				typeName = string.Format("DECIMAL({0},{1})", precision, scale);
				break;

			case PrimitiveTypeKind.Binary:
				typeName = "BLOB SUB_TYPE BINARY";
				break;

			case PrimitiveTypeKind.String:
				isUnicode = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.UnicodeFacetName, true);
				isFixedLength = MetadataHelpers.GetFacetValueOrDefault<bool>(type, MetadataHelpers.FixedLengthFacetName, false);
				length = MetadataHelpers.GetFacetValueOrDefault<int?>(type, MetadataHelpers.MaxLengthFacetName, null)
					?? (isUnicode ? FbProviderManifest.UnicodeVarcharMaxSize : FbProviderManifest.AsciiVarcharMaxSize);
				if (isFixedLength)
				{
					typeName = (isUnicode ? "CHAR(" : "CHAR(") + length + ")";
				}
				else
				{
					if (length > (isUnicode ? FbProviderManifest.UnicodeVarcharMaxSize : FbProviderManifest.AsciiVarcharMaxSize))
					{
						typeName = "BLOB SUB_TYPE TEXT";
					}
					else
					{
						typeName = (isUnicode ? "VARCHAR(" : "VARCHAR(") + length + ")";
					}
				}
				break;

			case PrimitiveTypeKind.DateTime:
				precision = MetadataHelpers.GetFacetValueOrDefault<byte>(type, MetadataHelpers.PrecisionFacetName, 4);
				typeName = (precision > 0 ? "TIMESTAMP" : "DATE");
				break;

			case PrimitiveTypeKind.Time:
				typeName = "TIME";
				break;

			case PrimitiveTypeKind.Guid:
				typeName = "CHAR(16) CHARACTER SET OCTETS";
				break;

			default:
				throw new NotSupportedException("Unsupported EdmType: " + primitiveType.PrimitiveTypeKind);
		}

		return typeName;
	}

	/// <summary>
	/// Handles the expression represending DbLimitExpression.Limit and DbSkipExpression.Count.
	/// If it is a constant expression, it simply does to string thus avoiding casting it to the specific value
	/// (which would be done if <see cref="Visit(DbConstantExpression)"/> is called)
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	private ISqlFragment HandleCountExpression(DbExpression e)
	{
		ISqlFragment result;

		if (e.ExpressionKind == DbExpressionKind.Constant)
		{
			//For constant expression we should not cast the value,
			// thus we don't go throught the default DbConstantExpression handling
			var sqlBuilder = new SqlBuilder();
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
	/// This is only the case when the DbExpressionKind is CrossApply or OuterApply.
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	bool IsApplyExpression(DbExpression e)
	{
		return (DbExpressionKind.CrossApply == e.ExpressionKind || DbExpressionKind.OuterApply == e.ExpressionKind);
	}

	/// <summary>
	/// This is used to determine if a particular expression is a Join operation.
	/// This is true for DbCrossJoinExpression and DbJoinExpression, the
	/// latter of which may have one of several different ExpressionKinds.
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	bool IsJoinExpression(DbExpression e)
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
	/// Constants, parameters, properties and internal functions as operators do not require brackets,
	/// everything else does.
	/// </summary>
	/// <param name="e"></param>
	/// <returns>true, if the expression needs brackets </returns>
	bool IsComplexExpression(DbExpression e)
	{
		switch (e.ExpressionKind)
		{
			case DbExpressionKind.Constant:
			case DbExpressionKind.ParameterReference:
			case DbExpressionKind.Property:
				return false;
			case DbExpressionKind.Function:
				return (!_functionNameToOperatorDictionary.ContainsKey((e as DbFunctionExpression).Function.Name));

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
	bool IsCompatible(SqlSelectStatement result, DbExpressionKind expressionKind)
	{
		switch (expressionKind)
		{
			case DbExpressionKind.Distinct:
				return result.First == null
					// The projection after distinct may not project all
					// columns used in the Order By
					&& result.OrderBy.IsEmpty;

			case DbExpressionKind.Filter:
				return result.Select.IsEmpty
						&& result.Where.IsEmpty
						&& result.GroupBy.IsEmpty
						&& result.First == null;

			case DbExpressionKind.GroupBy:
				return result.Select.IsEmpty
						&& result.GroupBy.IsEmpty
						&& result.OrderBy.IsEmpty
						&& result.First == null;

			case DbExpressionKind.Limit:
			case DbExpressionKind.Element:
				return result.First == null;

			case DbExpressionKind.Project:
				return result.Select.IsEmpty
						&& result.GroupBy.IsEmpty;

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
	/// Decorate with double quotes and escape double quotes inside in Firebird.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	internal static string QuoteIdentifier(string name)
	{
		Debug.Assert(!string.IsNullOrEmpty(name));
		// We assume that the names are not quoted to begin with.
		return "\"" + name.Replace("\"", "\"\"") + "\"";
	}

	/// <summary>
	/// Simply calls <see cref="VisitExpressionEnsureSqlStatement(DbExpression, bool)"/>
	/// with addDefaultColumns set to true
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e)
	{
		return VisitExpressionEnsureSqlStatement(e, true);
	}

	/// <summary>
	/// This is called from <see cref="GenerateSql(DbQueryCommandTree)"/> and nodes which require a
	/// select statement as an argument e.g. <see cref="Visit(DbIsEmptyExpression)"/>,
	/// <see cref="Visit(DbUnionAllExpression)"/>.
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
	/// DbLimitExpression needs to start the statement but not add the default columns
	/// </summary>
	/// <param name="e"></param>
	/// <param name="addDefaultColumns"></param>
	/// <returns></returns>
	SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e, bool addDefaultColumns)
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
				var inputVarName = "c";  // any name will do - this is my random choice.
				_symbolTable.EnterScope();

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

				result = VisitInputExpression(e, inputVarName, type, out var fromSymbol);
				AddFromSymbol(result, inputVarName, fromSymbol);
				_symbolTable.ExitScope();
				break;
		}

		if (addDefaultColumns && result.Select.IsEmpty)
		{
			AddDefaultColumns(result);
		}

		return result;
	}

	/// <summary>
	/// This method is called by <see cref="Visit(DbFilterExpression)"/> and
	/// <see cref="Visit(DbQuantifierExpression)"/>
	///
	/// </summary>
	/// <param name="input"></param>
	/// <param name="predicate"></param>
	/// <param name="negatePredicate">This is passed from <see cref="Visit(DbQuantifierExpression)"/>
	/// in the All(...) case.</param>
	/// <returns></returns>
	SqlSelectStatement VisitFilterExpression(DbExpressionBinding input, DbExpression predicate, bool negatePredicate)
	{
		var varName = GetShortenedName(input.VariableName);
		var result = VisitInputExpression(input.Expression,
			varName, input.VariableType, out var fromSymbol);

		// Filter is compatible with OrderBy
		// but not with Project, another Filter or GroupBy
		if (!IsCompatible(result, DbExpressionKind.Filter))
		{
			result = CreateNewSelectStatement(result, varName, input.VariableType, out fromSymbol);
		}

		_selectStatementStack.Push(result);
		_symbolTable.EnterScope();

		AddFromSymbol(result, varName, fromSymbol);

		if (negatePredicate)
		{
			result.Where.Append("NOT (");
		}
		result.Where.Append(predicate.Accept(this));
		if (negatePredicate)
		{
			result.Where.Append(")");
		}

		_symbolTable.ExitScope();
		_selectStatementStack.Pop();

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
	void WrapNonQueryExtent(SqlSelectStatement result, ISqlFragment sqlFragment, DbExpressionKind expressionKind)
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
	private static bool IsBuiltInFunction(EdmFunction function)
	{
		return MetadataHelpers.TryGetValueForMetadataProperty<bool>(function, "BuiltInAttribute");
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="function"></param>
	/// <param name="result"></param>
	void WriteFunctionName(SqlBuilder result, EdmFunction function)
	{
		var storeFunctionName = MetadataHelpers.TryGetValueForMetadataProperty<string>(function, "StoreFunctionNameAttribute");

		if (string.IsNullOrEmpty(storeFunctionName))
		{
			storeFunctionName = function.Name;
		}
		// If the function is a builtin (ie) the BuiltIn attribute has been
		// specified, then, the function name should not be quoted; additionally,
		// no namespace should be used.
		if (IsBuiltInFunction(function))
		{
			if (MetadataHelpers.IsCanonicalFunction(function))
			{
				switch (storeFunctionName)
				{
					case "BigCount":
						result.Append("COUNT");
						break;
					default:
						result.Append(storeFunctionName.ToUpperInvariant());
						break;
				}

			}
			else
			{
				result.Append(storeFunctionName);
			}

		}
		else
		{
			//result.Append(QuoteIdentifier((string)function.MetadataProperties["Schema"].Value ?? "dbo"));
			//result.Append(".");
			result.Append(QuoteIdentifier(storeFunctionName));
		}
	}

	/// <summary>
	/// Helper method for the Group By visitor
	/// Returns true if at least one of the aggregates in the given list
	/// has an argument that is not a <see cref="DbPropertyExpression"/>
	/// over <see cref="DbVariableReferenceExpression"/>
	/// </summary>
	/// <param name="aggregates"></param>
	/// <returns></returns>
	static bool NeedsInnerQuery(IList<DbAggregate> aggregates)
	{
		foreach (var aggregate in aggregates)
		{
			Debug.Assert(aggregate.Arguments.Count == 1);
			if (!IsPropertyOverVarRef(aggregate.Arguments[0]))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determines whether the given expression is a <see cref="DbPropertyExpression"/>
	/// over <see cref="DbVariableReferenceExpression"/>
	/// </summary>
	/// <param name="expression"></param>
	/// <returns></returns>
	static bool IsPropertyOverVarRef(DbExpression expression)
	{
		if (!(expression is DbPropertyExpression propertyExpression))
		{
			return false;
		}
		if (!(propertyExpression.Instance is DbVariableReferenceExpression varRefExpression))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Shortens the name of variable (tables, etc.).
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	internal string GetShortenedName(string name)
	{
		if (!_shortenedNames.TryGetValue(name, out var shortened))
		{
			shortened = BuildName(_shortenedNames.Count);
			_shortenedNames[name] = shortened;
		}
		return shortened;
	}

	internal static string BuildName(int index)
	{
		const int offset = 'A';
		const int length = 'Z' - offset;
		if (index <= length)
		{
			return ((char)(offset + index)).ToString();
		}
		else
		{
			return BuildName(index / length) + BuildName(index % length);
		}
	}

	#endregion
}
